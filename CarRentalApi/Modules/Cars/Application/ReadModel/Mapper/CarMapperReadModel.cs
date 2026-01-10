using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
namespace CarRentalApi.Modules.Customers.Application.Contracts.Mapping;

public static class CarMapperReadModel {
   public static CarDetails ToCarDetails(this Car car) => new(
      CarId: car.Id,
      Manufacturer: car.Manufacturer,
      Model: car.Model,
      LicensePlate: car.LicensePlate,
      Category: car.Category,
      IsInMaintenance: car.IsInMaintenance,
      IsRetired: car.IsRetired,
      CreatedAt: car.CreatedAt,
      RetiredAt: car.RetiredAt
   );
   
   public static CarListItem ToCarListItem(this Car car) => new(
      CarId: car.Id,
      Manufacturer: car.Manufacturer,
      Model: car.Model,
      LicensePlate: car.LicensePlate,
      Category: car.Category,
      IsInMaintenance: car.IsInMaintenance,
      IsRetired: car.IsRetired
   );
   
}
