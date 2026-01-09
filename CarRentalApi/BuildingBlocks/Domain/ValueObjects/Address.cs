using CarRentalApi.BuildingBlocks;
using CarRentalApi.Modules.Common.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.Errors;
namespace CarRentalApi.Modules.Customers.Domain.ValueObjects;

// Address is an owned value object without identity.
// It is immutable and fully replaced on change.
public sealed record class Address {
   
   public string Street     { get; init; } = string.Empty;
   public string PostalCode { get; init; } = string.Empty;
   public string City       { get; init; } = string.Empty;

   private Address(
      string street,
      string postalCode,
      string city
   ) {
      Street = street;
      PostalCode = postalCode;
      City = city;
   }

   public static Result<Address> Create(
      string street,
      string postalCode,
      string city
   ) {
      // Normalize input early
      street = street?.Trim() ?? string.Empty;
      postalCode = postalCode?.Trim() ?? string.Empty;
      city = city?.Trim() ?? string.Empty;
      
      if (string.IsNullOrWhiteSpace(street))
         return Result<Address>.Failure(AddressErrors.StreetIsRequired);

      if (string.IsNullOrWhiteSpace(postalCode))
         return Result<Address>.Failure(AddressErrors.PostalCodeIsRequired);

      if (string.IsNullOrWhiteSpace(city))
         return Result<Address>.Failure(AddressErrors.CityIsRequired);

      return Result<Address>.Success(new Address(street, postalCode, city));
   }
}
