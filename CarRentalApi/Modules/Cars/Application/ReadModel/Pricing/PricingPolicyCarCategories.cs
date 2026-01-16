using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using CarRentalApi.Modules.Cars.Ports.Inbound;
namespace CarRentalApi.Modules.Bookings.Application.Pricing;

public sealed class PricingPolicyCarCategories : IPricingPolicyCarCategories {
   
   // Basispreise pro Tag nach Fahrzeugkategorie
   private static decimal BasePerDay(CarCategory category) => category switch {
      CarCategory.Economy => 39m,
      CarCategory.Compact => 49m,
      CarCategory.Midsize => 59m,
      CarCategory.Suv => 79m,
      _ => 59m
   };
   
   // Rabattstaffel in Prozent nach Anzahl Tage
   private static int ResolveDiscountPct(int days) => days switch {
      >= 30 => 20,
      >= 14 => 15,
      >= 7  => 10,
      >= 3  => 5,
      _     => 0
   };
   
   public PricingQuote Calculate(
      CarCategory category, 
      DateTimeOffset start, 
      DateTimeOffset end
   ) {
      // reserved days
      var days = CalculateBillableDays(start, end);
      // price per day
      var perDay = BasePerDay(category);
      // discount percent
      var discount = ResolveDiscountPct(days);
      // total price without and with discount
      var gross = perDay * days;
      var total = gross * (100m - discount) / 100m;

      var quote = new PricingQuote(perDay, days, discount, total);
      return quote;
   }
   
   private static int CalculateBillableDays(DateTimeOffset start, DateTimeOffset end) {
      // didaktisch einfach: jede angefangene Nacht = 1 Tag
      // z.B. 2026-01-01 10:00 bis 2026-01-02 09:00 => 1 Tag
      var span = end - start;
      var days = (int)Math.Ceiling(span.TotalDays);
      return Math.Max(1, days);
   }
   
   private static decimal ResolveDiscountPercent(int days, IReadOnlyDictionary<int, decimal> tiers) {
      // nimmt die höchste passende Schwelle
      // Beispiel: tiers = {3:0.05, 7:0.10, 14:0.15, 30:0.25}
      var best = 0m;
      foreach (var kv in tiers) {
         var minDays = kv.Key;
         var pct = kv.Value;
         if (days >= minDays && pct > best) best = pct;
      }
      return best;
   }
}