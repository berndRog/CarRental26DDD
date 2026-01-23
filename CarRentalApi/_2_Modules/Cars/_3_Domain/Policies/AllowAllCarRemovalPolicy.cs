namespace CarRentalApi._2_Modules.Cars._3_Domain.Policies;

public sealed class AllowAllCarRemovalPolicy : ICarRemovalPolicy {
   public Task<bool> CheckAsync(Guid carId, CancellationToken ct) =>
      Task.FromResult(true);
}
