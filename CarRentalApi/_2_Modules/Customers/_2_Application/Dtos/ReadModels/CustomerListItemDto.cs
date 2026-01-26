namespace CarRentalApi._2_Modules.Customers._2_Application.Dtos.ReadModels;

/// <summary>
/// Lightweight projection for list views.
/// </summary>
public sealed record CustomerListItemDto(
   Guid Id,
   string Firstname,
   string Lastname,
   string Email,
   bool IsBlocked
);
