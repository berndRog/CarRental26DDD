
namespace CarRentalApi.Modules.Customers.Application.ReadModel.Dto;

/// <summary>
/// Lightweight projection for list views.
/// </summary>
public sealed record CustomerListItem(
   Guid Id,
   DateTimeOffset CreatedAt,
   bool IsBlocked
);
