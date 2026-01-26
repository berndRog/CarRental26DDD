namespace CarRentalApi._2_Modules.Customers._2_Application.Dtos.ReadModels;

/// <summary>
/// Detailed projection for customer detail views.
/// </summary>
public sealed record CustomerDetailDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string Email,
   string? Street,
   string? PostalCode,
   string? City,
   string? Country,
   DateTimeOffset CreatedAt,
   bool IsBlocked,
   DateTimeOffset? BlockedAt
);
