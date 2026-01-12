namespace CarRentalApi.Modules.Employees.Application.ReadModel;

public sealed record EmployeeSearchFilter(
   string? NameOrEmail,
   string? PersonnelNumber,
   int? AdminRights,          // Flags als int (oder AdminRights?)
   bool? IsActive
);

public sealed record EmployeeListItemDto(
   Guid EmployeeId,
   string PersonnelNumber,
   string FirstName,
   string LastName,
   string Email,
   bool IsActive,
   int AdminRights
);

public sealed record EmployeeDetailsDto(
   Guid EmployeeId,
   string PersonnelNumber,
   string FirstName,
   string LastName,
   string Email,
   bool IsActive,
   int AdminRights,
   DateTimeOffset CreatedAt,
   DateTimeOffset? DeactivatedAt
);
