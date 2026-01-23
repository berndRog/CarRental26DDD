namespace CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;

public sealed record CustomerProfileDto(
   string FirstName,
   string LastName,
   string Email,
   string? Street,
   string? PostalCode,
   string? City,
   string? Country
);
