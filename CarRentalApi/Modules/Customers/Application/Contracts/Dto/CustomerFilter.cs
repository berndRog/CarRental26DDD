
namespace CarRentalApi.Modules.Customers.Application.Contracts.Dto;

public sealed record CustomerFilter(
   string? FirstName = null,
   string? LastName  = null,
   string? Email     = null
);
