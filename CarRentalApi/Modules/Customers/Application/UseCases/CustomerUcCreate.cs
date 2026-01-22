using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Cars.Application.UseCases;

public sealed class CustomerUcCreate(
   ICustomerRepository _repository,
   IUnitOfWork _unitOfWork
) {
   
   public async Task<Result<Customer>> ExecuteAsync(
      string identitySubject,
      string firstName,
      string lastName,
      string email,
      string? birthdate,
      string? gender,
      DateTimeOffset updatedAt = default,
      string? street =  null,
      string? postalCode =  null,
      string? city = null,
      string? country = null,
      string? id = null,
      CancellationToken ct = default
   ) {
      // Domain factory: enforces domain invariants.
      var result = Customer.Create(firstName, lastName, email, 
         street, postalCode, city, updatedAt, id);
      if (result.IsFailure)
         return Result<Customer>.Failure(result.Error);
     
      var customer = result.Value!;
      _repository.Add(customer);
      await _unitOfWork.SaveAllChangesAsync("CustomerUcCreate", ct);
      
      return Result<Customer>.Success(customer);
   }
}