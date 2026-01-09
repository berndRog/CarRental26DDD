namespace CarRentalApi.BuildingBlocks;

public interface IClock {
   DateTimeOffset UtcNow { get; }
}