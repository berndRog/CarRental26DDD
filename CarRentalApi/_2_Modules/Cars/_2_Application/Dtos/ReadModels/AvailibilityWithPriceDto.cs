using CarRentalApi._2_Modules.Cars._2_Application.Dtos.Contracts;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
namespace CarRentalApi._2_Modules.Cars._2_Application.Dtos.ReadModels;

public sealed record AvailibilityWithPriceDto(
   CarCategory Category,
   int AvailableCars,
   decimal Total,
   int Days,
   decimal PricePerDay,
   int DiscountPercent,
   IReadOnlyList<CarContractDto> ExampleCars
);
