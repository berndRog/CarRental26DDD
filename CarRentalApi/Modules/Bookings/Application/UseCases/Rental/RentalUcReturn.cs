using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Rentals.Domain.Enums;
namespace CarRentalApi.Domain.UseCases.Rentals;

public sealed class RentalUcReturn(
   IRentalRepository _rentalRepo,
   IUnitOfWork _unitOfWork,
   IClock _clock,
   ILogger<RentalUcReturn> _logger
) {

   public async Task<Result> ExecuteAsync(
      Guid rentalId,
      RentalFuelLevel fuelIn,
      int kmIn,
      CancellationToken ct
   ) {
      // 1) Load aggregate (tracked)
      var rental = await _rentalRepo.FindByIdAsync(rentalId, ct);
      if (rental is null) {
         var fail = Result.Failure(RentalReadErrors.NotFound);
         fail.LogIfFailure(_logger,"RentalUcReturn.NotFound",
            new { rentalId });
         return fail;
      }

      // 2) Apply domain transition (pure)
      var returnAt = _clock.UtcNow;
      var result = rental.ReturnCar(
         returnAt: returnAt,
         fuelIn: fuelIn,
         kmIn: kmIn
      );

      if (result.IsFailure) {
         result.LogIfFailure(_logger, "RentalUcReturn.DomainRejected",
            new { rentalId = rental.Id, returnAt, fuelIn, kmIn });
         return Result.Failure(result.Error);
      }

      // 3) Persist changes
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Rental returned", ct);

      _logger.LogInformation(
         "RentalUcReturn done rentalId={id} savedRows={rows}",
         rental.Id, savedRows);

      return Result.Success();
   }
}