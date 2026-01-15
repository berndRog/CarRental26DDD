 
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;

namespace CarRentalApi.Modules.Employees.Domain.Errors;

/// <summary>
/// Domain-level error definitions for employee-related validation and business rules.
/// </summary>
public static class EmployeeErrors {

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
   
   public static readonly DomainErrors EmailIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Email Is Required",
         Message: "An email address must be provided."
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
   