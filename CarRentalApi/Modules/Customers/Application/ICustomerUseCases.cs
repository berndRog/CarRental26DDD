using CarRentalApi.BuildingBlocks;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Cars.Application;

public interface ICustomerUseCases {

   Task<Result<Customer>> CreateAsync(
      string firstName,
      string lastName,
      string email,
      string? street,
      string? postalCode,
      string? city,
      string? id,
      CancellationToken ct
   );

}