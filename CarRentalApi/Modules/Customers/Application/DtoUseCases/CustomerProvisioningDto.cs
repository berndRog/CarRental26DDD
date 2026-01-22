namespace CarRentalApi.Modules.Cars.Application.Dto;

/// <summary>
/// Detailed projection for customer detail views.
/// </summary>
public sealed record CustomerProvisioningDto(
   string IndentitySubject,
   string FirstName,
   string LastName,
   string Email,
   string PhoneNumber,
   string BirthDateIso,
   char Gender,
   string updatedAtIso,
   string? street,
   string? city,
   string? postalCode,
   string? country
);