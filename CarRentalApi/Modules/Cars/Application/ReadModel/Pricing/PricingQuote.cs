namespace CarRentalApi.Modules.Cars.Application.ReadModel.Dto;

public sealed record PricingQuote(
   decimal PricePerDay,
   int Days,
   int DiscountPercent,
   decimal Total
);