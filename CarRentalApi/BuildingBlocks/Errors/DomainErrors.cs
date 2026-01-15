using CarRentalApi.BuildingBlocks.Enums;
namespace CarRentalApi.BuildingBlocks.Errors;

/// <summary>
/// Represents a business/domain error.
/// Comparable to a sealed error type in Kotlin.
/// </summary>
public sealed record DomainErrors(
   ErrorCode Code,
   string? Title = "",
   string? Message = ""
) {
   // ----------------------------
   // Generic errors
   // ----------------------------
   public static readonly DomainErrors None =
      new(
         ErrorCode.BadRequest,
         Title: "No Error",
         Message: "No Error Has Occurred."
      );

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Resource Not Found",
         Message: "The Requested Resource Was Not Found."
      );

   public static readonly DomainErrors Forbidden =
      new(
         ErrorCode.Forbidden,
         Title: "Operation Forbidden",
         Message: "The Requested Operation Is Not Allowed."
      );

   
   public static readonly DomainErrors Invalid =
      new(
         ErrorCode.BadRequest,
         Title: "Value is invalid",
         Message: "The Value Is Not Valid."
      );
   
   public static readonly DomainErrors InvalidGuidFormat =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Guid Format",
         Message: "The Provided ReservationId Is Not A Valid GUID."
      );
   
   // Contact data
   public static readonly DomainErrors FirstNameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "First Name Is Required",
         Message: "A First Name Must Be Provided."
      );
   
   public static readonly DomainErrors InvalidFirstName =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid FirstName",
         Message: "The Provided FirstName is too shot or too long (2-100 chars)."
      );

   public static readonly DomainErrors LastNameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Last Name Is Required",
         Message: "A Last Name Must Be Provided."
      );

   public static readonly DomainErrors InvalidLastName =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid LastName",
         Message: "The Provided LastName is too shot or too long (2-100 chars)."
      );
   
   public static readonly DomainErrors EmailIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Email Is Required",
         Message: "An Email Address Must Be Provided."
      );

   public static readonly DomainErrors InvalidEmail =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid email address",
         Message: "The provided email address is not valid."
      );
 

}

