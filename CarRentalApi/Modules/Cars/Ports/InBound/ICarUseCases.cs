using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
namespace CarRentalApi.Modules.Cars.Application;

public interface ICarUseCases {

   Task<Result<Car>> CreateAsync(
      CarCategory category,
      string manufacturer,
      string model,
      string licensePlate,
      string? id,
      CancellationToken ct
   );
   

   public Task<Result> SendToMaintainanceAsync(
      Guid carId,
      CancellationToken ct
   );
   
   public Task<Result> ReturnFromMaintainanceAsync(
      Guid carId,
      CancellationToken ct
   );
   
   public Task<Result> RetireAsync(
      Guid carId,
      CancellationToken ct
   );

}