using CarRentalApi._2_Modules.Customers._1_Ports.Outbound;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
namespace CarRentalApi._2_Modules.Customers._2_Application.UseCases;

public sealed class CustomerUcCreate(
   ICustomerRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<CustomerUcCreate> _logger
) {
   
   public async Task<Result<Guid>> ExecuteAsync(
      string firstname,
      string lastname,
      string email,
      string subject,
      DateTimeOffset createdAt,
      string? id = null,
      string? street =  null,
      string? postalCode =  null,
      string? city = null,
      string? country = null,
      CancellationToken ct = default
   ) {
      // Domain factory: enforces domain invariants.
      var result = Customer.Create(
         firstname, 
         lastname, 
         email, 
         subject,
         createdAt, 
         id,
         street, 
         postalCode, 
         city, 
         country
      );
      if (result.IsFailure)
         return Result<Guid>.Failure(result.Error);
     
      // Add customer to repository (tracked by EF)
      var customer = result.Value!;
      _repository.Add(customer);
      
      // Persist via UnitOfWork
      var savedRows = await _unitOfWork.SaveAllChangesAsync("CustomerUcCreate", ct);
      
      
      _logger.LogInformation("CustomerUcCreate done CustomerId={id} savedRows={rows}",
         customer.Id, savedRows);
      
      return Result<Guid>.Success(customer.Id);
   }
}