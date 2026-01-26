namespace CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;

public sealed record CustomerProfileDto(
   string Firstname,
   string Lastname,
   string EmailString,
   
   string? Street,
   string? PostalCode,
   string? City,
   string? Country
);
