using CarRentalApi.BuildingBlocks.Enums;
namespace CarRentalApi.Modules.Cars.Application.ReadModel.Dto;

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
