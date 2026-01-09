using CarRentalApi.BuildingBlocks;
using CarRentalApi.Domain;
namespace CarRentalApiTest.Domain.Utils;

public sealed class FakeClock : IClock {
   public DateTimeOffset UtcNow { get; set; }
   public FakeClock(DateTimeOffset now) => UtcNow = now;
}