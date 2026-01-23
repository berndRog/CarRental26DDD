using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
namespace CarRentalApi._2_Modules.Cars._2_Application.Dtos;

public sealed record CarDto(
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