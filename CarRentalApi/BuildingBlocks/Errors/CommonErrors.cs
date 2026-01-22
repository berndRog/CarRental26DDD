using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;

public static class CommonErrors {
   public static readonly DomainErrors InvalidEmail =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid email address",
         Message: "The provided email address is not valid."
      );

   public static readonly DomainErrors InvalidPhone =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid phone number",
         Message: "The provided phone number is not valid."
      );
   
   public static readonly DomainErrors InvalidIdentitySubject =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid phone number",
         Message: "The provided phone number is not valid."
      );
   
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
