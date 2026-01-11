using CarRentalApi.Modules.Customers.Domain.ValueObjects;
namespace CarRentalApi.Modules.Customers.Application.contracts.Dto;

/// <summary>
/// Projection for customer contracts
/// </summary>
public sealed record class CustomerDto(
   string? Id,
   string FirstName,
   string LastName,
   string Email,
   DateTimeOffset CreatedAt,
   bool IsBlocked,
   Address? Address = null
);