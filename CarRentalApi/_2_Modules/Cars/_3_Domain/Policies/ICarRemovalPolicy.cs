namespace CarRentalApi._2_Modules.Cars._3_Domain.Policies;

public interface ICarRemovalPolicy {
   Task<bool> CheckAsync(Guid carId, CancellationToken ct);
}
