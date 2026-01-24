using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._4_BuildingBlocks.Domain.ValueObjects;

// Address is an owned value object without identity.
// It is immutable and fully replaced on change.
public sealed record class Address {
   
   public string Street     { get; init; } = string.Empty;
   public string PostalCode { get; init; } = string.Empty;
   public string City       { get; init; } = string.Empty;
   public string? Country   { get; init; } // Future use

   private Address(
      string street,
      string postalCode,
      string city,
      string? country = null
   ) {
      Street = street;
      PostalCode = postalCode;
      City = city;
      Country = country;
   }

   public static Result<Address> Create(
      string street,
      string postalCode,
      string city,
      string? country = null
   ) {
      // Normalize input early
      street = street.Trim() ?? string.Empty;
      postalCode = postalCode.Trim() ?? string.Empty;
      city = city.Trim() ?? string.Empty;
      country = country?.Trim();
      
      if (string.IsNullOrWhiteSpace(street))
         return Result<Address>.Failure(CommonErrors.StreetIsRequired);

      if (string.IsNullOrWhiteSpace(postalCode))
         return Result<Address>.Failure(CommonErrors.PostalCodeIsRequired);

      if (string.IsNullOrWhiteSpace(city))
         return Result<Address>.Failure(CommonErrors.CityIsRequired);

      return Result<Address>.Success(new Address(street, postalCode, city, country));
   }
}
