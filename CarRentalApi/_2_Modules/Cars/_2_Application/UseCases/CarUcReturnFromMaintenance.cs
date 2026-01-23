using CarRentalApi._2_Modules.Cars._1_Ports.Outbound;
using CarRentalApi._2_Modules.Cars._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.BuildingBlocks;
namespace CarRentalApi._2_Modules.Cars._2_Application.UseCases;

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