namespace CarRentalApi._4_BuildingBlocks._1_Ports.Inbound;

public interface IClock {
   DateTimeOffset UtcNow { get; }
}