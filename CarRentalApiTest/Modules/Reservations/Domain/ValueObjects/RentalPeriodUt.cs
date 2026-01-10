using CarRentalApi.Modules.Reservations.Domain.Errors;
using CarRentalApi.Modules.Reservations.Domain.ValueObjects;
namespace CarRentalApiTest.Modules.Reservations.Domain.ValueObjects;

public sealed class RentalPeriodTests {
   [Fact]
   public void Create_fails_when_start_is_after_or_equal_end() {
      var start = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
      var end = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

      var result = RentalPeriod.Create(start, end);

      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.InvalidPeriod.Code, result.Error.Code);
   }

   [Fact]
   public void Overlaps_returns_true_for_intersecting_periods() {
      var p1 = RentalPeriod.Create(
         new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
         new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)).Value!;

      var p2 = RentalPeriod.Create(
         new DateTimeOffset(2026, 1, 1, 11, 0, 0, TimeSpan.Zero),
         new DateTimeOffset(2026, 1, 1, 13, 0, 0, TimeSpan.Zero)).Value!;

      Assert.True(p1.Overlaps(p2));
   }
}