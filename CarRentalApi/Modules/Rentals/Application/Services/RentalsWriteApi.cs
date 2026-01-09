using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Cars.Application.Contracts;
using CarRentalApi.Modules.Rentals.Application.Contracts;
using CarRentalApi.Modules.Rentals.Application.Errors;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Modules.Rentals.Domain.Errors;
using CarRentalApi.Modules.Rentals.Infrastructure;
using CarRentalApi.Modules.Reservations.Application.Contracts;
namespace CarRentalApi.Modules.Rentals.Application.Services;

/// <summary>
/// Command facade for the Rentals bounded context.
/// 
/// Pick-up workflow:
/// - Load the confirmed reservation (Reservations BC)
/// - Select a suitable car (Cars/Fleet BC)
/// - Create the rental at pick-up (Rentals BC)
/// - Mark the reservation as rented/used (Reservations BC)
/// - Optionally mark the car as rented/in use (Cars/Fleet BC)
/// - Persist all changes in one unit of work (modular monolith)
/// 
/// Return workflow:
/// - Load the rental aggregate (Rentals BC)
/// - Execute the domain transition to "Returned" (Rentals BC)
/// - Optionally mark the car as available again (Cars/Fleet BC)
/// - Persist all changes in one unit of work
/// 
/// Important:
/// - Confirming a reservation does NOT create a rental.
/// - The rental is created only at pick-up, and only then the CarId becomes known.
/// </summary>

public sealed class RentalsWriteService(
   IReservationsReadApi _reservationsRead,
   IReservationsWriteApi _reservationsWrite,
   ICarsReadApi _carsRead,
   ICarsWriteApi? _carsWrite, // optional
   IRentalRepository _rentalRepository,
   IUnitOfWork _unitOfWork,
   IClock _clock
) : IRentalsWriteApi {

   public async Task<Result<Guid>> StartFromReservationAsync(
      Guid reservationId,
      CancellationToken ct
   ) {
      if (reservationId == Guid.Empty) {
         return Result<Guid>.Failure(RentalErrors.InvalidReservation);
      }

      // 1) Load confirmed reservation (Reservations BC)
      var reservationDto = await _reservationsRead.FindConfirmedByIdAsync(reservationId, ct);
      if (reservationDto is null) {
         // Either not found or not confirmed (depending on ReservationsReadApi contract)
         return Result<Guid>.Failure(RentalApplicationErrors.ReservationNotFound);
      }

      // 2) Find a suitable available car (Cars BC)
      var carResult = await _carsRead.FindAvailableCarAsync(
         reservationDto.Category,
         reservationDto.Start,
         reservationDto.End,
         ct
      );

      if (carResult.IsFailure) {
         return Result<Guid>.Failure(carResult.Error!);
      }

      if (carResult.Value is null) {
         // No candidate available => pick-up cannot proceed
         return Result<Guid>.Failure(RentalApplicationErrors.NoCarAvailable);
      }

      // 3) Create rental aggregate (domain)
      var pickupAt = _clock.UtcNow;

      // TODO: pass real inputs from controller/UI
      const int fuelLevelOut = 100;
      const int kmOut = 0;

      var rentalResult = Rental.CreateAtPickup(
         reservationId: reservationDto.ReservationId,
         customerId: reservationDto.CustomerId,
         carId: carResult.Value.Id,
         pickupAt: pickupAt,
         fuelLevelOut: fuelLevelOut,
         kmOut: kmOut
      );

      if (rentalResult.IsFailure) {
         return Result<Guid>.Failure(rentalResult.Error!);
      }

      var rental = rentalResult.Value!;

      // 4) Track rental (DbContext tracks it)
      _rentalRepository.Add(rental);

      // 5) Mark reservation as rented/used (Reservations BC)
      var resResult = await _reservationsWrite.MarkAsRentedAsync(reservationId, ct);
      if (resResult.IsFailure) {
         return Result<Guid>.Failure(resResult.Error!);
      }

      // 6) Optional: mark car as rented (Cars BC)
      if (_carsWrite is not null) {
         var carWriteResult = await _carsWrite.MarkAsRentedAsync(carResult.Value.Id, ct);
         if (carWriteResult.IsFailure) {
            return Result<Guid>.Failure(carWriteResult.Error!);
         }
      }

      // 7) Persist all changes (single transaction/UoW)
      await _unitOfWork.SaveAllChangesAsync("Rental created at pick-up", ct);

      return Result<Guid>.Success(rental.Id);
   }

   public async Task<Result> CloseAsync(
      Guid rentalId,
      CancellationToken ct
   ) {
      if (rentalId == Guid.Empty) {
         return Result.Failure(RentalErrors.InvalidId);
      }

      // 1) Load rental aggregate
      var rental = await _rentalRepository.FindByIdAsync(rentalId, ct);
      if (rental is null) {
         return Result.Failure(RentalApplicationErrors.RentalNotFound);
      }

      // 2) Execute domain behavior
      var returnAt = _clock.UtcNow;

      // TODO: pass real inputs from controller/UI
      const int fuelLevelIn = 100;
      var kmIn = rental.KmOut;

      var returnResult = rental.ReturnCar(
         returnAt: returnAt,
         fuelLevelIn: fuelLevelIn,
         kmIn: kmIn
      );

      if (returnResult.IsFailure) {
         return returnResult;
      }

      // 3) Optional: mark car as available again (Cars BC)
      if (_carsWrite is not null) {
         var carWriteResult = await _carsWrite.MarkAvailableAsync(rental.CarId, ct);
         if (carWriteResult.IsFailure) {
            return Result.Failure(carWriteResult.Error!);
         }
      }

      // 4) Persist
      await _unitOfWork.SaveAllChangesAsync("Rental closed at return", ct);

      return Result.Success();
   }
}
