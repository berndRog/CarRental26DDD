using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Infrastructure;
using CarRentalApi.Modules.Cars.Repositories;
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
      var result = Car.Create(category, manufacturer, model, licensePlate, id);
      if (result.IsFailure)
         return Result<Car>.Failure(result.Error);
      var car = result.Value!;
      
      // Use-case rule: Check if ID already exists
      var existing = await _repository.FindByIdAsync(car.Id, ct);
      if (existing != null) 
         return Result<Car>.Failure(CarErrors.IdAlreadyExists); 
      
      // Use-case rule: License plate must be unique.
      var exists = await _repository.ExistsLicensePlateAsync(car.LicensePlate, ct);
      if (exists)
         return Result<Car>.Failure(CarErrors.LicensePlateMustBeUnique);
      
      _repository.Add(car);
      await _unitOfWork.SaveAllChangesAsync("Car added", ct);

      _logger.LogInformation("CarUcCreate done carId={id}", car.Id);
      return Result<Car>.Success(result.Value!);
   }
}

