using CarRentalApi.BuildingBlocks.Enums;
namespace CarRentalApi.Modules.Cars.Application.ReadModel.Dto;

/// <summary>
/// Detailed projection for car detail views.
/// </summary>
public sealed record CarDetails(
   Guid CarId,
   string Manufacturer,
   string Model,
   string LicensePlate,
   CarCategory Category,
   bool IsInMaintenance,
   bool IsRetired,
   DateTimeOffset CreatedAt,
   DateTimeOffset? RetiredAt
);
