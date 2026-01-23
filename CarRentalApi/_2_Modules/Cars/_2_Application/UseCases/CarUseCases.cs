using CarRentalApi._2_Modules.Cars._1_Ports.Inbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi.BuildingBlocks;
namespace CarRentalApi._2_Modules.Cars._2_Application.UseCases;

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