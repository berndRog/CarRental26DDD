using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Infrastructure;
using CarRentalApi.Modules.Rentals.Application.Errors;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Modules.Rentals.Domain.Errors;
using CarRentalApi.Modules.Rentals.Infrastructure;
using CarRentalApi.Modules.Reservations.Application;
using CarRentalApi.Modules.Reservations.Domain.Enums;
using CarRentalApi.Modules.Reservations.Infrastructure;
namespace CarRentalApi.Domain.UseCases.Rentals;


public sealed class RentalUcPickup(
   IReservationRepository _reservationRepo,
   ICustomerRepository _customerRepo,
   ICarRepository _carRepo,
   IRentalRepository _rentalRepo,
   IUnitOfWork _unitOfWork,
   IClock _clock,
   ILogger<RentalUcPickup> _logger
) {
   public async Task<Result<Rental>> ExecuteAsync(
      Guid reservationId,
      Guid customerId,
      Guid carId,
      int fuelLevelOut,
      int kmOut,
      CancellationToken ct
   ) {
      _logger.LogInformation(
         "RentalUcPickup start reservationId={reservationId} customerId={customerId} carId={carId}",
         reservationId, customerId, carId
      );

      // --- Load & existence checks (DDD via repos) ---
      var reservation = await _reservationRepo.FindByIdAsync(reservationId, ct);
      if (reservation is null)
         return Result<Rental>.Failure(RentalApplicationErrors.ReservationNotFound);

      var customer = await _customerRepo.FindByIdAsync(customerId, ct);
      if (customer is null)
         return Result<Rental>.Failure(RentalApplicationErrors.CustomerNotFound);

      var car = await _carRepo.FindByIdAsync(carId, ct);
      if (car is null)
         return Result<Rental>.Failure(RentalApplicationErrors.CarNotFound);

      if (reservation.Status != ReservationStatus.Confirmed)
          return Result<Rental>.Failure(RentalApplicationErrors.ReservationInvalidStatus);

      var pickupAt = _clock.UtcNow;

      // --- Create Rental aggregate root ---
      var rentalResult = Rental.CreateAtPickup(
         reservationId: reservationId,
         customerId: customerId,
         carId: carId,
         pickupAt: pickupAt,
         fuelLevelOut: fuelLevelOut,
         kmOut: kmOut
      );

      if (rentalResult.IsFailure)
         return Result<Rental>.Failure(rentalResult.Error);

      var rental = rentalResult.Value;
      _rentalRepo.Add(rental);

      var saved = await _unitOfWork.SaveAllChangesAsync("RentalUcPickup", ct);
      if (!saved)
         return Result<Rental>.Failure(RentalApplicationErrors.RentalSaveFailed);

      _logger.LogInformation("RentalUcPickup success rentalId={rentalId}", rental.Id);
      return Result<Rental>.Success(rental);
   }
}