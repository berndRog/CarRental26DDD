using CarRentalApi._2_Modules.Cars._2_Application.Dtos;
using CarRentalApi._2_Modules.Cars._3_Domain.Aggregates;
namespace CarRentalApi._2_Modules.Cars._2_Application.Mappings;

public static class CarMapping { 
   
   public static CarDto ToCarDto(this Car car) => new(
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
   
   public static CarListItemDto ToCarListItem(this Car car) => new(
      CarId: car.Id,
      Manufacturer: car.Manufacturer,
      Model: car.Model,
      LicensePlate: car.LicensePlate,
      Category: car.Category,
      IsInMaintenance: car.IsInMaintenance,
      IsRetired: car.IsRetired
   );
   
}
