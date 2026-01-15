using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;

public static class CommonErrors {

   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid ID",
         Message: "The provided ID is invalid."
      );

   public static readonly DomainErrors FirstNameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "First name required",
         Message: "A first name must be provided."
      );

   public static readonly DomainErrors InvalidFirstName =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid first name",
         Message: "The provided first name is too short or too long (2–100 characters)."
      );

   public static readonly DomainErrors LastNameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Last name required",
         Message: "A last name must be provided."
      );

   public static readonly DomainErrors InvalidLastName =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid last name",
         Message: "The provided last name is too short or too long (2–100 characters)."
      );

   public static readonly DomainErrors EmailIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Email required",
         Message: "An email address must be provided."
      );

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
   
   public static readonly DomainErrors CreatedAtIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Creation Timestamp Required",
         Message: "The creation timestamp (createdAt) must be provided."
      );
}
