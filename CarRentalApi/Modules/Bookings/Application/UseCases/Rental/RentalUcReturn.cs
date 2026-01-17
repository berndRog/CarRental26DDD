using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;
using CarRentalApi.Modules.Bookings.Application.UseCases.Dto;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Cars.Application.Contracts;

namespace CarRentalApi.Domain.UseCases.Rentals;

/// <summary>
/// Return workflow for an active rental.
///
/// Steps:
/// - Load rental aggregate (tracked)
/// - Apply domain transition (return car)
/// - Mark car as available (Fleet/Cars BC)
/// - Commit once (UnitOfWork)
///
/// Notes:
/// - All operations run within the same DbContext / transaction
/// - Domain invariants are enforced inside the Rental aggregate
/// </summary>
public sealed class RentalUcReturn(
   IRentalRepository _rentalRepo,
   ICarWriteContract _carsWrite,
   IUnitOfWork _unitOfWork,
   ILogger<RentalUcReturn> _logger
) {

   /// <summary>
   /// Returns a rented car and closes the rental.
   ///
   /// Business meaning:
   /// - Completes the rental lifecycle
   /// - Makes the car available for future rentals
   ///
   /// Preconditions:
   /// - The rental must exist
   /// - The rental must be in Active state
   ///
   /// Side effects:
   /// - Updates the rental aggregate
   /// - Updates the car state in the Fleet/Cars BC
   ///
   /// Returns:
   /// - Success if the rental was successfully returned
   /// - NotFound if the rental does not exist
   /// - Conflict if the rental cannot be returned in its current state
   /// </summary>
   public async Task<Result> ExecuteAsync(
      RentalReturnDto returnDto,
      CancellationToken ct
   ) {
      // 1) Load rental aggregate (tracked)
      var rental = await _rentalRepo.FindByIdAsync(returnDto.RentalId, ct);
      if (rental is null) 
         return Result.Failure(RentalApplicationErrors.NotFound)
            .LogIfFailure(_logger, "RentalUcReturn.NotFound", new { returnDto.RentalId });

      // 2) Apply domain transition
      var resultReturnCar = rental.ReturnCar(
         returnAt: returnDto.ReturnAt,
         fuelIn: returnDto.FuelIn,
         kmIn: returnDto.KmIn
      );

      if (resultReturnCar.IsFailure) 
         return resultReturnCar.LogIfFailure(_logger, "RentalUcReturn.DomainRejected",
            new { rentalId = rental.Id, returnDto.ReturnAt, returnDto.FuelIn, returnDto.KmIn });
      
      // 3) Mark car as available (Fleet/Cars BC)
      var resultCarWrite = await _carsWrite.MarkAsAvailableAsync(rental.CarId, ct);
      if (resultCarWrite.IsFailure) 
         return Result.Failure(resultCarWrite.Error)
            .LogIfFailure(_logger, "RentalUcReturn.MarkCarAvailableFailed",
               new { rentalId = rental.Id, carId = rental.CarId });

      // 4) Commit once
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Rental returned", ct);

      _logger.LogInformation(
         "RentalUcReturn done rentalId={rentalId} carId={carId} savedRows={rows}",
         rental.Id, rental.CarId, savedRows
      );

      return Result.Success();
   }
}
