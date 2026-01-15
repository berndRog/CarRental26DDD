using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Cars.Application.Contracts;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Ports.Outbound;
namespace CarRentalApi.Modules.Cars.Infrastructure.Adapters;

public sealed class CarWriteContractServiceEf(
   ICarRepository _carRepository,
   IUnitOfWork _unitOfWork,
   ILogger<CarWriteContractServiceEf> _logger
) : ICarWriteContract {

   public async Task<Result> SendToMaintenanceAsync(
      Guid carId,
      CancellationToken ct
   ) {
      var car = await _carRepository.FindByIdAsync(carId, ct);
      if (car is null)
         return Result.Failure(CarErrors.NotFound);

      var result = car.SendToMaintenance();
      if (result.IsFailure)
         return result;

      await _unitOfWork.SaveAllChangesAsync("Send to CarStatus.Maintainance", ct);

      _logger.LogInformation("Car {carId} sent to maintenance", carId);

      return Result.Success();
   }

   /// <summary>
   /// Returns a car from maintenance back to the active fleet.
   /// 
   /// Business meaning:
   /// - The car becomes available for future rentals again
   /// </summary>
   public async Task<Result> ReturnFromMaintenanceAsync(
      Guid carId,
      CancellationToken ct
   ) {
      var car = await _carRepository.FindByIdAsync(carId, ct);
      if (car is null)
         return Result.Failure(CarErrors.NotFound);

      var result = car.ReturnFromMaintenance();
      if (result.IsFailure)
         return result;

      await _unitOfWork.SaveAllChangesAsync("Return from CarStatus.Maintainance", ct);

      _logger.LogInformation("Car {carId} returned from maintenance", carId);

      return Result.Success();
   }

   /// <summary>
   /// Marks a car as "in use" when a rental is created at pick-up.
   /// 
   /// Business meaning:
   /// - The car is currently assigned to an active rental
   /// - The car must not be assigned to another rental at the same time
   /// </summary>
   public async Task<Result> MarkAsRentedAsync(
      Guid carId,
      CancellationToken ct
   ) {
      var car = await _carRepository.FindByIdAsync(carId, ct);
      if (car is null)
         return Result.Failure(CarErrors.NotFound);

      // "InUse" == Rented
      var result = car.MarkAsRented();
      if (result.IsFailure)
         return result;

      await _unitOfWork.SaveAllChangesAsync("Marked as CarStatus.Rented", ct);

      _logger.LogInformation("Car {carId} marked as rented", carId);

      return Result.Success();
   }

   /// <summary>
   /// Marks a car as "Available" again after a rental has been closed.
   /// 
   /// Business meaning:
   /// - The car is no longer assigned to an active rental
   /// - The car may be rented again unless it is in maintenance
   /// </summary>
   public async Task<Result> MarkAsAvailableAsync(
      Guid carId,
      CancellationToken ct
   ) {
      var car = await _carRepository.FindByIdAsync(carId, ct);
      if (car is null)
         return Result.Failure(CarErrors.NotFound);

      var result = car.MarkAsAvailable();
      if (result.IsFailure)
         return result;

      await _unitOfWork.SaveAllChangesAsync("Marked as CarStatus.Availible", ct);

      _logger.LogInformation(
         "Car {carId} marked as available",
         carId
      );

      return Result.Success();
   }
}