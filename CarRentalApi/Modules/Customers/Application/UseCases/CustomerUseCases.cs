using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Cars.Application.UseCases;

public class CustomerUseCases(
   CustomerUcCreate createUc
): ICustomerUseCases {
   
   public Task<Result<Customer>> CreateAsync(
      string firstName,
      string lastName,
      string email,
      string? street,
      string? postalCode,
      string? city,
      string? id,
      CancellationToken ct
   ) => createUc.ExecuteAsync(firstName, lastName, email, street, postalCode, city, id, ct);
   
}