using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
namespace CarRentalApi._2_Modules.Cars._2_Application.Dtos;

/// <summary>
/// Lightweight projection for list views.
/// </summary>
public sealed record CarListItemDto(
   Guid CarId,
   string Manufacturer,
   string Model,
   string LicensePlate,
   CarCategory Category,
   bool IsInMaintenance,
   bool IsRetired
);
