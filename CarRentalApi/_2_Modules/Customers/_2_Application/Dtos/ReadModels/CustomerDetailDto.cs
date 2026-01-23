namespace CarRentalApi._2_Modules.Customers._2_Application.Dtos.ReadModels;

/// <summary>
/// Detailed projection for customer detail views.
/// </summary>
public sealed record CustomerDetailDto(
   Guid Id,
   DateTimeOffset CreatedAt,
   bool IsBlocked,
   DateTimeOffset? BlockedAt
);
