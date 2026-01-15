using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;

namespace CarRentalApi.Modules.Customers.Domain.Errors;

public static class CredentialsErrors {
   public static readonly DomainErrors PasswordEmpty =
      new(
         ErrorCode.BadRequest,
         Title: "Password is empty",
         Message: "The provided password must not be empty."
      );

   public static readonly DomainErrors InvalidCredentials =
      new(
         ErrorCode.Unauthorized,
         Title: "Invalid credentials",
         Message: "The provided password is incorrect."
      );

   public static readonly DomainErrors CredentialsNotSet =
      new(
         ErrorCode.BadRequest,
         Title: "Credentials not set",
         Message: "No credentials are stored for this customer."
      );

   public static readonly DomainErrors PasswordPolicyViolation =
      new(
         ErrorCode.BadRequest,
         Title: "Password policy violation",
         Message:
         "Password must be 6-24 characters and contain at least 1 uppercase, 1 lowercase, 1 digit and 1 special character."
      );
}