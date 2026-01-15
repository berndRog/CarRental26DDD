using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Application.UseCases.Block;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Cars.Application.UseCases;

public class CustomerUseCases(
   CustomerUcCreate createUc,
   CustomerUcBlock blockUc
): ICustomerUseCases {
   
   public Task<Result<Customer>> CreateAsync(
      string firstName,
      string lastName,
      string email,
      DateTimeOffset createdAt,
      string? street,
      string? postalCode,
      string? city,
      string? id,
      CancellationToken ct
   ) => createUc.ExecuteAsync(firstName, lastName, email, createdAt, 
      street, postalCode, city, id, ct);
   
   public Task<Result> BlockAsync(
      Guid id,
      DateTimeOffset blockedAt,
      CancellationToken ct
   ) => blockUc.ExecuteAsync(id, blockedAt, ct);
   
}