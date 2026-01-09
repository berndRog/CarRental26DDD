namespace CarRentalApi.Modules.Cars.Domain.Policies;

public interface ICarRemovalPolicy {
   Task<bool> CheckAsync(Guid carId, CancellationToken ct);
}
