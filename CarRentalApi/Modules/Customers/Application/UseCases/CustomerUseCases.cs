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
      DateTimeOffset createdAt,
      string? id,
      CancellationToken ct
   ) => createUc.ExecuteAsync(createdAt, id, ct);
   
   public Task<Result> BlockAsync(
      Guid id,
      DateTimeOffset blockedAt,
      CancellationToken ct
   ) => blockUc.ExecuteAsync(id, blockedAt, ct);
   
}