using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;
using CarRentalApi.Modules.Bookings.Domain;
namespace CarRentalApi.Domain.UseCases.Rentals;

public sealed class RentalUcReturn(
   IRentalRepository _rentalRepo,
   IUnitOfWork _unitOfWork,
   IClock _clock,
   ILogger<RentalUcReturn> _logger
) {
   
   public async Task<Result> ExecuteAsync(
      Guid rentalId,
      int fuelLevelIn,
      int kmIn,
      CancellationToken ct
   ) {

      // fetch rental from database and track it (via EF Core DbContext)
      var rental = await _rentalRepo.FindByIdAsync(rentalId, ct);
      if (rental is null)
         return Result.Failure(RentalReadErrors.NotFound);

      // domain model operation
      var returnAt = _clock.UtcNow;
      var result = rental.ReturnCar(
         returnAt: returnAt,
         fuelLevelIn: fuelLevelIn,
         kmIn: kmIn
      );
      if (result.IsFailure)
         return Result.Failure(result.Error);

      // Persist changes to database
      var savedRows = await _unitOfWork.SaveAllChangesAsync("RentalUcReturn", ct);

      _logger.LogInformation(
         "RentalUcReturn success rentalId={id} savedRows={rows}",
         rental.Id, savedRows
      );
      return Result.Success();
   }
}