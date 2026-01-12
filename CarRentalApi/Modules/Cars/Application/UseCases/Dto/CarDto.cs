using CarRentalApi.BuildingBlocks.Enums;
namespace CarRentalApi.Modules.Cars.Application.UseCases.Dto;

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