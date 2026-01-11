using CarRentalApi.BuildingBlocks;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Cars.Application;

public interface ICustomerUseCases {

   Task<Result<Customer>> CreateAsync(
      DateTimeOffset createdAt,
      string? id,
      CancellationToken ct
   );
   
   Task<Result> BlockAsync(
      Guid id,
      DateTimeOffset blockedAt,
      CancellationToken ct
   );

}