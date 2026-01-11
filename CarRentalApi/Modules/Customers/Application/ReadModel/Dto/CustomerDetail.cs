using CarRentalApi.Modules.Customers.Domain.ValueObjects;
namespace CarRentalApi.Modules.Customers.Application.ReadModel.Dto;

/// <summary>
/// Detailed projection for customer detail views.
/// </summary>
public sealed record CustomerDetail(
   Guid Id,
   string FirstName,
   string LastName,
   string Email,
   DateTimeOffset CreatedAt,
   Address? Address,
   bool IsBlocked,
   DateTimeOffset? BlockedAt
);
