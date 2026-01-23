namespace CarRentalApi._2_Modules.Employees._2_Application.Dtos.ReadModel;

public sealed record EmployeeSearchFilter(
   string? NameOrEmail,
   string? PersonnelNumber,
   int? AdminRights,          // Flags als int (oder AdminRights?)
   bool? IsActive
);