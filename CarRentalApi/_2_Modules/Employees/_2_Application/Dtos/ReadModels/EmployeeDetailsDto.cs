namespace CarRentalApi._2_Modules.Employees._2_Application.Dtos.ReadModel;

public sealed record EmployeeDetailsDto(
   Guid EmployeeId,
   string PersonnelNumber,
   string Firstname,
   string Lastname,
   string Email,
   bool IsActive,
   int AdminRights,
   DateTimeOffset CreatedAt,
   DateTimeOffset? DeactivatedAt
);
