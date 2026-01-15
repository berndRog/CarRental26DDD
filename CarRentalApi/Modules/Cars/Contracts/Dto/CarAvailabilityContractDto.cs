using CarRentalApi.BuildingBlocks.Enums;
namespace CarRentalApi.Modules.Cars.Application.Contracts.Dto;

public sealed record CarAvailabilityContractDto(
   CarCategory Category,
   int AvailableCars,
   IReadOnlyList<CarContractDto> ExampleCars
);