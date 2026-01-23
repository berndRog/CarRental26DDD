using CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi.BuildingBlocks;
using CarRentalApi.Modules.Customers._1_Ports.Inbound;
namespace CarRentalApi._2_Modules.Customers._2_Application.UseCases;

public class CustomerUseCases(
   CustomerUcCreate createUc,
   CustomerUcProvisioned provisionedUc,
   CustomerUcProfile profileUc,
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
      string? country,
      string? id,
      CancellationToken ct
   ) => createUc.ExecuteAsync(firstName, lastName, email, createdAt, 
      street, postalCode, city, country, id, ct);
   
   
   public Task<Result<Guid>> Provisioned(
      CancellationToken ct
   ) => provisionedUc.ExecuteAsync(ct);
   
   public Task<Result<CustomerProfileDto>> Profile(
      CustomerProfileDto customerProfileDto,
      CancellationToken ct
   ) => profileUc.ExecuteAsync(customerProfileDto, ct);
   
   public Task<Result> BlockAsync(
      Guid id,
      DateTimeOffset blockedAt,
      CancellationToken ct
   ) => blockUc.ExecuteAsync(id, blockedAt, ct);
}