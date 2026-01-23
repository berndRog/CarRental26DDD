using CarRentalApi._4_BuildingBlocks._1_Ports.Inbound;
namespace CarRentalApi._4_BuildingBlocks._4_Infrastructure;

public sealed class CarSystemClock : IClock {
   public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}