using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Domain.Policies;
using CarRentalApi.Modules.Cars.Infrastructure;
using CarRentalApi.Modules.Cars.Repositories;
namespace CarRentalApi.Modules.Cars.Application.UseCases;

public sealed class CarUcRetire(
   ICarRepository _repository,
   IUnitOfWork _unitOfWork,
   ICarRemovalPolicy _policy, // prepared for cross-aggregate rules
   ILogger<CarUcRetire> _logger
) {
   
   public async Task<Result> ExecuteAsync(
      Guid carId, 
      CancellationToken ct
   ) {

      var car = await _repository.FindByIdAsync(carId, ct);
      if (car is null) {
         _logger.LogWarning("CarUcRetire rejected carId={id} not found", carId);
         return Result.Failure(CarErrors.NotFound);
      }

      // Optional later: block removal if active reservations exist
      var canRemove = await _policy.CheckAsync(carId, ct);
      if (!canRemove) {
         // TODO later: introduce a specific domain error like CarErrors.CannotRemoveWithActiveReservations
         _logger.LogWarning("CarUcRetire rejected carId={id} reason=active_reservations", carId);
         return Result.Failure(CarErrors.InvalidStatusTransition);
      }

      var result = car.Retire();
      if (result.IsFailure) {
         _logger.LogWarning("CarUcRetire rejected carId={id} errorCode={code}",
            carId, result.Error.Code);
         return result;
      }

      await _unitOfWork.SaveAllChangesAsync("Car retired", ct);

      _logger.LogDebug("CarUcRetire done carId={id}", carId);
      return Result.Success();
   }
}