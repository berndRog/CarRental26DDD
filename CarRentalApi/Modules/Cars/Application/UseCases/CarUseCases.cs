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
   public Task<Result<Guid>> CreateAsync(
      string manufacturer,
      string model,
      string licensePlate,
      CarCategory category,
      DateTimeOffset createdAt,
      string? id,
      CancellationToken ct
   ) => createUc.ExecuteAsync(
      manufacturer: manufacturer,
      model: model,
      licensePlate: licensePlate,
      category: category,
      createdAt: createdAt,
      id: id,
      ct: ct
   );
   
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