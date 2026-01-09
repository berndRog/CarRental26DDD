using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Infrastructure;
namespace CarRentalApi.Modules.Cars.Application.UseCases;

public sealed class CarUcReturnFromMaintenance(
   ICarRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<CarUcReturnFromMaintenance> _logger
) {
   
   public async Task<Result> ExecuteAsync(
      Guid carId, 
      CancellationToken ct
   ) {

      // fetch car from database and save it to repository
      var car = await _repository.FindByIdAsync(carId, ct);
      if (car is null) {
         _logger.LogWarning("CarUcReturnFromMaintenance rejected, carId={id} not found", carId);
         return Result.Failure(CarErrors.NotFound);
      }

      // domain operation
      var result = car.ReturnFromMaintenance();
      if (result.IsFailure) {
         _logger.LogWarning("CarUcReturnFromMaintenance rejected carId={id} errorCode={code}",
            carId, result.Error.Code);
         return result;
      }
      
      // save changes to database
      await _unitOfWork.SaveAllChangesAsync("Car returned from maintenance", ct); 
      return Result.Success();
   }
}