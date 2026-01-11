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
      DateTimeOffset createdAt,
      string? id,
      CancellationToken ct
   ) {
      // // Create Address value object if all address fields are provided
      // Address? address = null;
      // if (!string.IsNullOrWhiteSpace(street) &&
      //     !string.IsNullOrWhiteSpace(postalCode) &&
      //     !string.IsNullOrWhiteSpace(city)
      //    ) {
      //    var addressResult = Address.Create(street, postalCode, city);
      //    if (addressResult.IsFailure)
      //       return Result<Customer>.Failure(addressResult.Error);
      //    address = addressResult.Value;
      // }
      
      // Domain factory: enforces domain invariants.
      var result = Customer.Create(createdAt, id);
      if (result.IsFailure)
         return Result<Customer>.Failure(result.Error);
     
      var customer = result.Value!;
      _repository.Add(customer);
      await _unitOfWork.SaveAllChangesAsync("CustomerUcCreate", ct);
      
      return Result<Customer>.Success(customer);
   }
}