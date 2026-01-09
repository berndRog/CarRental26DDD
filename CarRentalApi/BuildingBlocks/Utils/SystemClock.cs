using CarRentalApi.Domain;
namespace CarRentalApi.BuildingBlocks.Utils;

public sealed class SystemClock : IClock {
   public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}