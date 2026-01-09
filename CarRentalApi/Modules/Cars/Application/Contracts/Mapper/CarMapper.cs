using CarRentalApi.Modules.Cars.Application.Contracts.Dto;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
namespace CarRentalApi.Modules.Customers.Application.Contracts.Mapping;

public static class CarMapper {
   
   public static CarDto ToDto(this Car car) => new(
      car.Id,
      car.Manufacturer,
      car.Model,
      car.LicensePlate,
      car.Category,
      car.Status
   );
}
