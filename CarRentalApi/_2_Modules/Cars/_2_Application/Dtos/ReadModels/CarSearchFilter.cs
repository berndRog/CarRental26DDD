using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
namespace CarRentalApi._2_Modules.Cars._2_Application.Dtos.ReadModels;

/// <summary>
/// Filter options for car search screens.
/// Keep it UI/endpoint oriented (not domain logic).
/// </summary>
public sealed record CarSearchFilter(
   string? SearchText = null,
   CarCategory? Category = null,
   bool? IsInMaintenance = null,
   bool? IsRetired = null
);
