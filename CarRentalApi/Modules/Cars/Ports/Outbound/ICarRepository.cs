using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Enums;
namespace CarRentalApi.Modules.Cars.Ports.Outbound;

public interface ICarRepository {
   
   Task<Car?> FindByIdAsync(Guid id, CancellationToken ct);
   Task<bool> ExistsLicensePlateAsync(string licensePlate, CancellationToken ct);
   Task<int> CountCarsInCategoryAsync(CarCategory category, CancellationToken ct);
   
   Task<IReadOnlyList<Car>> SelectByAsync(
      CarCategory? category,
      CarStatus? status,
      CancellationToken ct
   );
   
   void Add(Car car);
}