using CarRentalApi.Modules.Customers.Domain.ValueObjects;
namespace CarRentalApi.Modules.Customers.Application.contracts.Dto;

/// <summary>
/// Projection for customer contracts
/// </summary>
public sealed record class CustomerContractDto(
   string? Id,
   string? Identity,
   DateTimeOffset CreatedAt,
   bool IsBlocked
);