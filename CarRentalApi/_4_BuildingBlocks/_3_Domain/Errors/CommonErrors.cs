using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
namespace CarRentalApi._4_BuildingBlocks._3_Domain.Errors;

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
         Title: "Invalid IdentitySubject",
         Message: "The provided subject (sub) is not valid."
      );

   public static readonly DomainErrors Forbidden =
      new(
         ErrorCode.Forbidden,
         Title: "Access denied",
         Message: "You are authenticated but not allowed to perform this action."
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