using CarRentalApi.BuildingBlocks.Enums;
namespace CarRentalApi.Modules.Cars.Application.ReadModel.Dto;

/// <summary>
/// Lightweight projection for list views.
/// </summary>
public sealed record CarListItem(
   Guid CarId,
   string Manufacturer,
   string Model,
   string LicensePlate,
   CarCategory Category,
   bool IsInMaintenance,
   bool IsRetired
);
