using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Ports.Inbound;
namespace CarRentalApi.Modules.Cars.Application.UseCases;

public class CarUseCases(
   CarUcCreate createUc,
   CarUcSendToMaintenance sendToMaintenanceUc,
   CarUcReturnFromMaintenance returnFromMaintenanceUc,
   CarUcRetire retireUc
): ICarUseCases {
   public Task<Result<Car>> CreateAsync(
      CarCategory category,
      string manufacturer,
      string model,
      string licensePlate,
      string? id,
      CancellationToken ct
   ) => createUc.ExecuteAsync(category, manufacturer, model, licensePlate, id, ct);
   
   public Task<Result> SendToMaintainanceAsync(
      Guid carId,
      CancellationToken ct
   ) => sendToMaintenanceUc.ExecuteAsync(carId, ct);
   
   public Task<Result> ReturnFromMaintainanceAsync(
      Guid carId,
      CancellationToken ct
   ) => returnFromMaintenanceUc.ExecuteAsync(carId, ct);
   
   public Task<Result> RetireAsync(
      Guid carId,
      CancellationToken ct
   ) => retireUc.ExecuteAsync(carId, ct);

}