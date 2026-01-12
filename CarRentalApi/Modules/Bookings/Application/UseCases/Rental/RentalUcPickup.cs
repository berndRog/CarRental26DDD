using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.Errors;
using CarRentalApi.Modules.Cars.Application.Contracts;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Modules.Rentals.Domain.Enums;

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
      RentalFuelLevel FuelOut,
      int kmOut,
      CancellationToken ct
   ) {
      // Guard: invalid id
      if (reservationId == Guid.Empty) {
         var fail = Result<Guid>.Failure(ReservationErrors.InvalidId);
         fail.LogIfFailure(_logger, "RentalUcPickup.InvalidReservationId",
            new { reservationId });
         return fail;
      }

      // 1) Load reservation (same BC => repository)
      var reservation = await _reservationRepository.FindByIdAsync(reservationId, ct);
      if (reservation is null) {
         var fail = Result<Guid>.Failure(RentalReadErrors.ReservationNotFound);
         fail.LogIfFailure(_logger, "RentalUcPickup.ReservationNotFound",
            new { reservationId });
         return fail;
      }

      // Optional early guard for clearer error
      if (reservation.Status != ReservationStatus.Confirmed) {
         var fail = Result<Guid>.Failure(RentalReadErrors.ReservationInvalidStatus);
         fail.LogIfFailure(_logger, "RentalUcPickup.ReservationInvalidStatus",
            new { reservationId, reservation.Status });
         return fail;
      }

      // 2) Find an available car (Fleet/Cars BC)
      var carResult = await _carsRead.FindAvailableCarAsync(
         reservation.CarCategory,
         reservation.Period.Start,
         reservation.Period.End,
         ct
      );

      if (carResult.IsFailure) {
         carResult.LogIfFailure(_logger, "RentalUcPickup.FindAvailableCarFailed",
            new { reservationId, reservation.CarCategory, reservation.Period }
         );
         return Result<Guid>.Failure(carResult.Error);
      }

      if (carResult.Value is null) {
         var fail = Result<Guid>.Failure(RentalReadErrors.NoCarAvailable);
         fail.LogIfFailure(_logger, "RentalUcPickup.NoCarAvailable",
            new { reservationId, reservation.CarCategory, reservation.Period }
         );
         return fail;
      }

      var pickupAt = _clock.UtcNow;

      // 3) Create rental aggregate (domain)
      var rentalResult = Rental.CreateAtPickup(
         reservationId: reservation.Id,
         customerId: reservation.CustomerId,
         carId: carResult.Value.Id,
         pickupAt: pickupAt,
         fuelOut: FuelOut,
         kmOut: kmOut
      );

      if (rentalResult.IsFailure) {
         rentalResult.LogIfFailure(_logger, "RentalUcPickup.CreateRentalRejected",
            new {
               reservationId,
               carId = carResult.Value.Id,
               pickupAt,
               fuelLevelOut,
               kmOut
            });
         return Result<Guid>.Failure(rentalResult.Error);
      }

      var rental = rentalResult.Value!;

      // 4) Assign rental to reservation (domain behavior, idempotent)
      var assignResult = reservation.AssignRental(rental.Id);
      if (assignResult.IsFailure) {
         assignResult.LogIfFailure(_logger, "RentalUcPickup.AssignRentalRejected",
            new { reservationId, rentalId = rental.Id });
         return Result<Guid>.Failure(assignResult.Error);
      }

      // 5) Persist both aggregates in one UoW
      _rentalRepository.Add(rental);

      // 6) Optional: mark car as rented
      if (_carsWrite is not null) {
         var carWriteResult = await _carsWrite.MarkAsRentedAsync(carResult.Value.Id, ct);
         if (carWriteResult.IsFailure) {
            carWriteResult.LogIfFailure(
               _logger,
               "RentalUcPickup.MarkCarAsRentedFailed",
               new { reservationId, carId = carResult.Value.Id }
            );
            return Result<Guid>.Failure(carWriteResult.Error);
         }
      }

      // 7) Commit once
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Rental created at pick-up", ct);

      _logger.LogInformation(
         "RentalUcPickup done reservationId={id} rentalId={id} carId={id} savedRows={rows}",
         reservation.Id, rental.Id, carResult.Value.Id, savedRows);

      return Result<Guid>.Success(rental.Id);
   }
}