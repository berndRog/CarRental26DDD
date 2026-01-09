using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Common.Domain.Errors;

/// <summary>
/// Domain-level error definitions for Address value object validation.
/// </summary>
public static class AddressErrors {

   public static readonly DomainErrors StreetIsRequired =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Street Is Required",
         Message: "The Street Must Not Be Empty."
      );

   public static readonly DomainErrors PostalCodeIsRequired =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Postal Code Is Required",
         Message: "The Postal Code Must Not Be Empty."
      );

   public static readonly DomainErrors CityIsRequired =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "City Is Required",
         Message: "The City Must Not Be Empty."
      );
}
