using CarRentalApi._2_Modules.Cars._2_Application.Dtos.Contracts;
using CarRentalApi._2_Modules.Cars._3_Domain.Aggregates;
namespace CarRentalApi._2_Modules.Cars._2_Application.Mappings;

public static class CarContractMapping {
   
   public static CarContractDto ToCarContractDto(this Car car) => new(
      car.Id,
      car.Manufacturer,
      car.Model,
      car.LicensePlate,
      car.Category,
      car.Status
   );
}
