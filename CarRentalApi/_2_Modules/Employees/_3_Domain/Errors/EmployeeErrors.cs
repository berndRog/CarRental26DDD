using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._2_Modules.Employees._3_Domain.Errors;

/// <summary>
/// Domain-level error definitions for employee-related validation and business rules.
/// </summary>
public static class EmployeeErrors {
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

   public static readonly DomainErrors EmailInvalidFormat =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Email Format",
         Message: "The email address has an invalid format."
      );

   public static readonly DomainErrors EmailMustBeUnique =
      new(
         ErrorCode.Conflict,
         Title: "Email Must Be Unique",
         Message: "An employee with the given email address already exists."
      );

   public static readonly DomainErrors InvalidEmail =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid email address",
         Message: "The provided email address is not valid."
      );

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Employee Not Found",
         Message: "The requested employee does not exist."
      );

   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Employee Id",
         Message: "The provided employee id is invalid."
      );

   public static readonly DomainErrors PersonnelNumberIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Personnel Number Is Required",
         Message: "A personnel number must be provided."
      );

   public static readonly DomainErrors PersonnelNumberInvalidFormat =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Personnel Number Format",
         Message: "The personnel number has an invalid format."
      );

   public static readonly DomainErrors PersonnelNumberMustBeUnique =
      new(
         ErrorCode.Conflict,
         Title: "Personnel Number Must Be Unique",
         Message: "An employee with the given personnel number already exists."
      );

   public static readonly DomainErrors AdminRightsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Admin Rights Required",
         Message: "An employee must have at least one admin right."
      );

   public static readonly DomainErrors AdminRightsInvalid =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Admin Rights",
         Message: "The provided admin rights value is invalid."
      );

   public static readonly DomainErrors InvalidAdminRightsBitmask =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Admin Rights Bitmask",
         Message: "The provided admin rights value contains undefined or unsupported flag bits."
      );

   public static readonly DomainErrors CreatedAtIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Creation Timestamp Required",
         Message: "The creation timestamp (createdAt) must be provided."
      );

   public static readonly DomainErrors AlreadyDeactivated =
      new(
         ErrorCode.Conflict,
         Title: "Employee Already Deactivated",
         Message: "The employee is already deactivated."
      );
   public static readonly DomainErrors AlreadyActive =
      new(
         ErrorCode.Conflict,
         Title: "Employee Already Active",
         Message: "The employee is already active."
      );

   public static readonly DomainErrors DeactivatedAtIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Deactivation Timestamp Required",
         Message: "The deactivation timestamp (deactivatedAt) must be provided when creating an employee."
      );
}