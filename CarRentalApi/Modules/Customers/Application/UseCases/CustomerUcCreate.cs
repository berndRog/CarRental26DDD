using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
namespace CarRentalApi.Modules.Cars.Application.UseCases;

public sealed class CustomerUcCreate(
   ICustomerRepository _repository,
   IUnitOfWork _unitOfWork
) {
   
   public async Task<Result<Customer>> ExecuteAsync(
      string firstName,
      string lastName,
      string email,
      DateTimeOffset createdAt = default,
      string? street =  null,
      string? postalCode =  null,
      string? city = null,
      string? id = null,
      CancellationToken ct = default
   ) {
      // Domain factory: enforces domain invariants.
      var result = Customer.Create(firstName, lastName, email, 
         street, postalCode, city, createdAt, id);
      if (result.IsFailure)
         return Result<Customer>.Failure(result.Error);
     
      var customer = result.Value!;
      _repository.Add(customer);
      await _unitOfWork.SaveAllChangesAsync("CustomerUcCreate", ct);
      
      return Result<Customer>.Success(customer);
   }
}