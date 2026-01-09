using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Employees.Domain.Errors;

/// <summary>
/// Domain-level error definitions for employee-related validation rules.
/// </summary>
public static class EmployeeErrors {

   public static readonly DomainErrors PersonnelNumberIsRequired =
      new(
         ErrorCode.BadReqest,
         Title: "Personnel Number Is Required",
         Message: "A Personnel Number Must Be Provided."
      );

   public static readonly DomainErrors PersonnelNumberInvalidFormat =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Personnel Number Format",
         Message: "The Personnel Number Has An Invalid Format."
      );

   public static readonly DomainErrors AdminRightsRequired =
      new(
         ErrorCode.BadReqest,
         Title: "Admin Rights Required",
         Message: "An Admin Must Have At Least One Admin Right."
      );
}
