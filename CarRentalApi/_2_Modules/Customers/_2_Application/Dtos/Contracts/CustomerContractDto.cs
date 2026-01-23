namespace CarRentalApi._2_Modules.Customers._2_Application.Dtos.Contracts;

/// <summary>
/// Projection for customer contracts
/// </summary>
public sealed record class CustomerContractDto(
   Guid   Id,
   string FirstName,
   string LastName,
   string Email
);