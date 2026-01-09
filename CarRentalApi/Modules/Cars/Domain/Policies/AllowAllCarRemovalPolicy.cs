using CarRentalApi.Domain;
namespace CarRentalApi.Modules.Cars.Domain.Policies;

public sealed class AllowAllCarRemovalPolicy : ICarRemovalPolicy {
   public Task<bool> CheckAsync(Guid carId, CancellationToken ct) =>
      Task.FromResult(true);
}
