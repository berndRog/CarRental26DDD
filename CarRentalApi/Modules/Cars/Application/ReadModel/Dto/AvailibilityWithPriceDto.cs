using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Cars.Application.Contracts.Dto;
namespace CarRentalApi.Modules.Bookings.Application.Pricing.Dto;

public sealed record AvailibilityWithPriceDto(
   CarCategory Category,
   int AvailableCars,
   decimal Total,
   int Days,
   decimal PricePerDay,
   int DiscountPercent,
   IReadOnlyList<CarContractDto> ExampleCars
);
