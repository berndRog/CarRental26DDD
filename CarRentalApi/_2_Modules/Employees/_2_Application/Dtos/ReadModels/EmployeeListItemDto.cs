namespace CarRentalApi._2_Modules.Employees._2_Application.Dtos.ReadModel;

public sealed record EmployeeListItemDto(
   Guid EmployeeId,
   string PersonnelNumber,
   string FirstName,
   string LastName,
   string Email,
   bool IsActive,
   int AdminRights
);