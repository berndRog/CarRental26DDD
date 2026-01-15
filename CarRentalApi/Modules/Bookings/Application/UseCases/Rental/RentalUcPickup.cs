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
/// - Guard: ensure no rental exists yet for this reservation
/// - Select a suitable available car (Fleet/Cars BC)
/// - Create rental
/// - Persist rental
/// - Mark car as rented (Fleet/Cars BC)
/// - Commit once (UnitOfWork)
///
/// Notes:
/// - DB enforces 1:0..1 Reservation <-> Rental via UNIQUE(Rentals.ReservationId)
/// </summary>
public sealed class RentalUcPickup(
   IReservationRepository _reservationRepository,
   IRentalRepository _rentalRepository,
   ICarReadContract _carsRead,
   ICarWriteContract _carsWrite,
   IUnitOfWork _unitOfWork,
   IClock _clock,
   ILogger<RentalUcPickup> _logger
) {

   public async Task<Result<Guid>> ExecuteAsync(
      Guid reservationId,
      RentalFuelLevel fuelOut,
      int kmOut,
      CancellationToken ct
   ) {
      if (reservationId == Guid.Empty) {
         return Result<Guid>.Failure(ReservationErrors.InvalidId)
            .LogIfFailure(_logger, "RentalUcPickup.InvalidReservationId", new { reservationId });
      }

      // 1) Load reservation (same BC => repository)
      var reservation = await _reservationRepository.FindByIdAsync(reservationId, ct);
      if (reservation is null)
         return Result<Guid>.Failure(RentalApplicationErrors.ReservationNotFound)
            .LogIfFailure(_logger, "RentalUcPickup.ReservationNotFound", new { reservationId });

      // Guard: reservation must be confirmed for pick-up
      if (reservation.Status != ReservationStatus.Confirmed)
         return Result<Guid>.Failure(RentalApplicationErrors.ReservationInvalidStatus)
            .LogIfFailure(_logger, "RentalUcPickup.ReservationInvalidStatus",
               new { reservationId, reservation.Status });

      // Guard: prevent duplicate pick-up / parallel requests
      var alreadyExists = await _rentalRepository.ExistsForReservationAsync(reservationId, ct);
      if (alreadyExists)
         return Result<Guid>.Failure(RentalApplicationErrors.RentalAlreadyExistsForReservation)
            .LogIfFailure(_logger, "RentalUcPickup.AlreadyExistsForReservation", new { reservationId });

      // 2) Find an available car (Fleet/Cars BC)
      var carResult = await _carsRead.FindAvailableCarAsync(
         reservation.CarCategory,
         reservation.Period.Start,
         reservation.Period.End,
         ct
      );

      if (carResult.IsFailure)
         return Result<Guid>.Failure(carResult.Error)
            .LogIfFailure(_logger, "RentalUcPickup.FindAvailableCarFailed",
               new { reservationId, reservation.CarCategory, reservation.Period });

      if (carResult.Value is null)
         return Result<Guid>.Failure(RentalApplicationErrors.NoCarAvailable)
            .LogIfFailure(_logger, "RentalUcPickup.NoCarAvailable",
               new { reservationId, reservation.CarCategory, reservation.Period });

      // 3) Create rental aggregate (domain)
      var pickupAt = _clock.UtcNow;
      var rentalResult = Rental.CreateAtPickup(
         reservationId: reservation.Id,
         customerId: reservation.CustomerId,
         carId: carResult.Value.CarId,
         pickupAt: pickupAt,
         fuelOut: fuelOut,
         kmOut: kmOut
      );

      if (rentalResult.IsFailure)
         return Result<Guid>.Failure(rentalResult.Error)
            .LogIfFailure(_logger, "RentalUcPickup.CreateRentalRejected",
               new { reservationId, carId = carResult.Value.CarId, pickupAt, fuelOut, kmOut });

      // 4) Persist rental (same UoW/DbContext)
      var rental = rentalResult.Value!;
      _rentalRepository.Add(rental);

      // 5) Mark car as rented (same UoW/DbContext)
      var carWriteResult = await _carsWrite.MarkAsRentedAsync(carResult.Value.CarId, ct);
      if (carWriteResult.IsFailure)
         return Result<Guid>.Failure(carWriteResult.Error)
            .LogIfFailure(_logger, "RentalUcPickup.MarkCarAsRentedFailed",
               new { reservationId, carId = carResult.Value.CarId });

      // 6) Commit once (UnitOfWork)
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Rental created at pick-up", ct);

      _logger.LogInformation(
         "RentalUcPickup done reservationId={reservationId} rentalId={rentalId} carId={carId} savedRows={rows}",
         reservation.Id, rental.Id, carResult.Value.CarId, savedRows);

      return Result<Guid>.Success(rental.Id);
   }
}
