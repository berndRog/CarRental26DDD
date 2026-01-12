using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.Errors;
using CarRentalApi.Modules.Cars.Application.Contracts;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
namespace CarRentalApi.Modules.Bookings.Application.UseCases;

/// <summary>
/// Pick-up workflow inside the Booking BC (Reservation + Rental).
///
/// Steps:
/// - Load reservation (must be Confirmed)
/// - Select a suitable available car (Fleet/Cars BC)
/// - Create rental
/// - Assign rental to reservation (idempotent)
/// - Optionally mark car as rented
/// - Commit once (UnitOfWork)
/// </summary>
public sealed class RentalUcPickup(
   IReservationRepository _reservationRepository,
   IRentalRepository _rentalRepository,
   ICarReadContract _carsRead,
   ICarWriteContract? _carsWrite, // optional
   IUnitOfWork _unitOfWork,
   IClock _clock,
   ILogger<RentalUcPickup> _logger
) {

   public async Task<Result<Guid>> ExecuteAsync(
      Guid reservationId,
      int fuelLevelOut,
      int kmOut,
      CancellationToken ct
   ) {
      if (reservationId == Guid.Empty)
         return Result<Guid>.Failure(ReservationErrors.InvalidId);

      // 1) Load reservation (same BC => repository)
      var reservation = await _reservationRepository.FindByIdAsync(reservationId, ct);
      if (reservation is null)
         return Result<Guid>.Failure(RentalReadErrors.ReservationNotFound);

      // (Optional extra guard: AssignRental already checks Status == Confirmed,
      // but doing it here improves error clarity / early exit)
      if (reservation.Status != ReservationStatus.Confirmed)
         return Result<Guid>.Failure(RentalReadErrors.ReservationInvalidStatus);

      // 2) Find an available car (Fleet/Cars BC)
      // Your ICarsReadApi signature earlier used (category, period).
      // If yours uses (category, start, end), adapt accordingly.
      var carResult = await _carsRead.FindAvailableCarAsync(
         reservation.CarCategory,
         reservation.Period.Start,
         reservation.Period.End,
         ct
      );

      if (carResult.IsFailure)
         return Result<Guid>.Failure(carResult.Error);

      if (carResult.Value is null)
         return Result<Guid>.Failure(RentalReadErrors.NoCarAvailable);

      var pickupAt = _clock.UtcNow;

      // 3) Create rental aggregate (domain)
      var rentalResult = Rental.CreateAtPickup(
         reservationId: reservation.Id,
         customerId: reservation.CustomerId,
         carId: carResult.Value.Id,
         pickupAt: pickupAt,
         fuelLevelOut: fuelLevelOut,
         kmOut: kmOut
      );

      if (rentalResult.IsFailure)
         return Result<Guid>.Failure(rentalResult.Error);

      var rental = rentalResult.Value!;

      // 4) Assign rental to reservation (domain behavior, idempotent)
      var assignResult = reservation.AssignRental(rental.Id);
      if (assignResult.IsFailure)
         return Result<Guid>.Failure(assignResult.Error);

      // 5) Persist both aggregates in one UoW
      _rentalRepository.Add(rental);

      // 6) Optional: mark car as rented
      if (_carsWrite is not null) {
         var carWriteResult = await _carsWrite.MarkAsRentedAsync(carResult.Value.Id, ct);
         if (carWriteResult.IsFailure)
            return Result<Guid>.Failure(carWriteResult.Error);
      }

      await _unitOfWork.SaveAllChangesAsync("Rental created at pick-up", ct);

      _logger.LogInformation(
         "RentalUcPickup completed. reservationId={ReservationId} rentalId={RentalId} carId={CarId}",
         reservation.Id, rental.Id, carResult.Value.Id
      );

      return Result<Guid>.Success(rental.Id);
   }
}
