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
   public async Task<Result<Guid>> ExecuteAsync(
      string manufacturer,
      string model,
      string licensePlate,
      CarCategory category,
      DateTimeOffset createdAt,
      string? id,
      CancellationToken ct
   ) {
      
      // Domain factory: enforces invariants (format, required fields, etc.)
      var result = Car.Create(manufacturer, model, licensePlate, category, createdAt, id);
      if (result.IsFailure) {
         result.LogIfFailure(_logger, "CarUcCreate.DomainRejected",
            new { category, manufacturer, model, licensePlate, id });
         return Result<Guid>.Failure(result.Error);
      }
      
      var car = result.Value!;
      
      // Use-case rule: Check if ID already exists (only relevant if id was provided / derived).
      var existing = await _repository.FindByIdAsync(car.Id, ct);
      if (existing is not null) {
         var failure = Result<Guid>.Failure(CarErrors.IdAlreadyExists);
         failure.LogIfFailure(_logger, "CarUcCreate.IdAlreadyExists",
            new { carId = car.Id, category, manufacturer, model, licensePlate });
         return failure;
      }

      // Use-case rule: License plate must be unique.
      var exists = await _repository.ExistsLicensePlateAsync(car.LicensePlate, ct);
      if (exists) {
         var failure = Result<Guid>.Failure(CarErrors.LicensePlateMustBeUnique);
         failure.LogIfFailure(_logger, "CarUcCreate.LicensePlateMustBeUnique",
            new { carId = car.Id, licensePlate = car.LicensePlate });
         return failure;
      }
      
      // Add car to repository (tracked by EF)
      _repository.Add(car);
      
      // Persist via UnitOfWork
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Car added", ct);
      _logger.LogInformation("CarUcCreate done CarId={id} savedRows={rows}",
         car.Id, savedRows);

      return Result<Guid>.Success(car.Id);
   }
}
