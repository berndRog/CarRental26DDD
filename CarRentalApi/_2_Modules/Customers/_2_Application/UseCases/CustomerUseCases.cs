using CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi.Modules.Customers._1_Ports.Inbound;
namespace CarRentalApi._2_Modules.Customers._2_Application.UseCases;

public class CustomerUseCases(
   CustomerUcCreate createUc,
   CustomerUcProvision provisionUc,
   CustomerUcProfile profileUc,
   CustomerUcBlock blockUc
): ICustomerUseCases {
   
   public Task<Result<Guid>> CreateAsync(
      string firstname,
      string lastname,
      string email,
      string subject,
      DateTimeOffset createdAt,
      string? id,
      string? street,
      string? postalCode,
      string? city,
      string? country,
      CancellationToken ct
   ) => createUc.ExecuteAsync(firstname, lastname, email, subject, 
      createdAt, id, street, postalCode, city, country, ct);
   
   
   public Task<Result<Guid>> Provisioned(
      string? id,
      CancellationToken ct
   ) => provisionUc.ExecuteAsync(id, ct);
   
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