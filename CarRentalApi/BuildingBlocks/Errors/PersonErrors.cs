using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Common.Domain.Errors;

/// <summary>
/// Domain-level error definitions for person-related validation rules.
/// </summary>
public static class PersonErrors {

   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Invalid Person ReservationId",
         Message: "The Provided Person ReservationId Is Invalid."
      );

   public static readonly DomainErrors FirstNameIsRequired =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "First Name Is Required",
         Message: "A First Name Must Be Provided."
      );

   public static readonly DomainErrors LastNameIsRequired =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Last Name Is Required",
         Message: "A Last Name Must Be Provided."
      );

   public static readonly DomainErrors EmailIsRequired =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Email Is Required",
         Message: "An Email Address Must Be Provided."
      );

   public static readonly DomainErrors EmailInvalidFormat =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Invalid Email Format",
         Message: "The Email Address Has An Invalid Format."
      );
}

