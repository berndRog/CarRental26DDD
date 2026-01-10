
namespace CarRentalApi.Modules.Customers.Application.ReadModel.Dto;

/// <summary>
/// Lightweight projection for list views.
/// </summary>
public sealed record CustomerListItem(
   Guid CustomerId,
   string FirstName,
   string LastName,
   string Email,
   DateTime CreatedAt
   // Optional: bool IsBlocked
);
