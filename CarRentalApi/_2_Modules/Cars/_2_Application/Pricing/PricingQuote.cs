namespace CarRentalApi._2_Modules.Cars._2_Application.Pricing;

public sealed record PricingQuote(
   decimal PricePerDay,
   int Days,
   int DiscountPercent,
   decimal Total
);