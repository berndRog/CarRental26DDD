namespace CarRentalApi.Modules.Customers.Application.contracts.Dto;

public sealed record class CustomerDto(
   string? Id,
   string FirstName,
   string LastName,
   string Email,
   DateTimeOffset CreatedAt,
   string? Street,
   string? PostalCode,
   string? City
);