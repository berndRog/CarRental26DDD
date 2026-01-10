using CarRentalApi.BuildingBlocks;
namespace CarRentalApiTest;

public sealed class FakeClock : IClock {
   public DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;
   
   public FakeClock(DateTimeOffset? utcNow = null) {
      if (utcNow.HasValue) {
         UtcNow = utcNow.Value;
      }
   }
}