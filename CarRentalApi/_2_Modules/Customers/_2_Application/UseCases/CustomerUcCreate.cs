using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.Domain;
namespace CarRentalApi._2_Modules.Customers._2_Application.UseCases;

public sealed class CustomerUcCreate(
   ICustomerRepository _repository,
   IUnitOfWork _unitOfWork
) {
   
   public async Task<Result<Customer>> ExecuteAsync(
      string firstName,
      string lastName,
      string email,
      DateTimeOffset createdAt,
      string? street =  null,
      string? postalCode =  null,
      string? city = null,
      string? country = null,
      string? id = null,
      CancellationToken ct = default
   ) {
      // Domain factory: enforces domain invariants.
      var result = Customer.Create(
         firstName, 
         lastName, 
         email, 
         street, 
         postalCode, 
         city, 
         country,
         createdAt, 
         id
      );
      if (result.IsFailure)
         return Result<Customer>.Failure(result.Error);
     
      var customer = result.Value!;
      _repository.Add(customer);
      await _unitOfWork.SaveAllChangesAsync("CustomerUcCreate", ct);
      
      return Result<Customer>.Success(customer);
   }
}