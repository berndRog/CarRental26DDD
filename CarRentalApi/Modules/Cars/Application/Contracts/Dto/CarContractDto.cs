using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Cars.Domain.Enums;
namespace CarRentalApi.Modules.Cars.Application.Contracts.Dto;

public sealed record CarContractDto(
   Guid CarId,
   string Manufacturer,
   string Model,
   string LicensePlate,
   CarCategory Category,
   CarStatus Status
);


