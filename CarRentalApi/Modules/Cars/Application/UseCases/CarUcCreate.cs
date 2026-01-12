using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Ports.Outbound;
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
      
      // Domain factory: enforces invariants (format, required fields, etc.)
      var result = Car.Create(category, manufacturer, model, licensePlate, id);
      if (result.IsFailure) {
         result.LogIfFailure(_logger, "CarUcCreate.DomainRejected",
            new { category, manufacturer, model, licensePlate, id });
         return Result<Car>.Failure(result.Error);
      }
      
      var car = result.Value!;
      
      // Use-case rule: Check if ID already exists (only relevant if id was provided / derived).
      var existing = await _repository.FindByIdAsync(car.Id, ct);
      if (existing is not null) {
         var fail = Result<Car>.Failure(CarErrors.IdAlreadyExists);
         fail.LogIfFailure(_logger, "CarUcCreate.IdAlreadyExists",
            new { carId = car.Id, category, manufacturer, model, licensePlate });
         return fail;
      }

      // Use-case rule: License plate must be unique.
      var exists = await _repository.ExistsLicensePlateAsync(car.LicensePlate, ct);
      if (exists) {
         var fail = Result<Car>.Failure(CarErrors.LicensePlateMustBeUnique);
         fail.LogIfFailure(_logger, "CarUcCreate.LicensePlateMustBeUnique",
            new { carId = car.Id, licensePlate = car.LicensePlate });
         return fail;
      }
      
      // Add car to repository (tracked by EF)
      _repository.Add(car);
      
      // Persist state
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Car added", ct);
      _logger.LogInformation("CarUcCreate done CarId={id} savedRows={rows}",
         car.Id, savedRows);

      return Result<Car>.Success(result.Value!);
   }
}
