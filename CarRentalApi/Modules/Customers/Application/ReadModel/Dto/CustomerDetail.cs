using CarRentalApi.Modules.Customers.Domain.ValueObjects;
namespace CarRentalApi.Modules.Customers.Application.ReadModel.Dto;

/// <summary>
/// Detailed projection for customer detail views.
/// </summary>
public sealed record CustomerDetail(
   Guid Id,
   DateTimeOffset CreatedAt,
   bool IsBlocked,
   DateTimeOffset? BlockedAt
);
