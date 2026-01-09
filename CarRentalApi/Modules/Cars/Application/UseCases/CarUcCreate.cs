using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Infrastructure;
namespace CarRentalApi.Modules.Cars.Application.UseCases;

public sealed class CarUcCreate(
   ICarRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<CarUcCreate> _logger
) {
   
   public async Task<Result<Car>> ExecuteAsync(
      CarCategory category,
      string manufacturer,
      string model,
      string licensePlate,
      string? id,
      CancellationToken ct
   ) {
      _logger.LogInformation(
         "CarUcCreate start category={cat} licensePlate={plate}",
         category, licensePlate
      );

      // Use-case rule: license plate must be unique.
      var exists = await _repository.ExistsLicensePlateAsync(licensePlate.Trim(), ct);
      if (exists)
         return Result<Car>.Failure(CarErrors.LicensePlateMustBeUnique);

      var result = Car.Create(category, manufacturer, model, licensePlate, id);
      if (result.IsFailure)
         return Result<Car>.Failure(result.Error!);

      _repository.Add(result.Value!);
      await _unitOfWork.SaveAllChangesAsync("Car added", ct);

      _logger.LogInformation("CarUcCreate done carId={id}", result.Value!.Id);
      return Result<Car>.Success(result.Value!);
   }
}

