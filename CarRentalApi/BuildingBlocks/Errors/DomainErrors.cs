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
         ErrorCode.BadReqest,
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

   public static readonly DomainErrors InvalidGuidFormat =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Guid Format",
         Message: "The Provided Id Is Not A Valid GUID."
      );
}

