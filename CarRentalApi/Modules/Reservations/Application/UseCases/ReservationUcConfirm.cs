using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Reservations.Domain;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
using CarRentalApi.Modules.Reservations.Domain.Enums;
using CarRentalApi.Modules.Reservations.Domain.Errors;
using CarRentalApi.Modules.Reservations.Infrastructure;
namespace CarRentalApi.Modules.Reservations.Application.UseCases;

public sealed class ReservationUcConfirm(
   IReservationRepository _repository,
   IUnitOfWork _unitOfWork,
   IReservationConflictPolicy _conflicts,
   ILogger<ReservationUcConfirm> _logger,
   IClock _clock
) {
   
   public async Task<Result> ExecuteAsync(Guid reservationId, CancellationToken ct) {

      // Fetch reservation (aggregate) from _repository.
      var reservation = await _repository.FindByIdAsync(reservationId, ct);
      if (reservation is null) 
         return Result.Failure(ReservationErrors.NotFound);
      
      
      // Use-case / domain-service rule:
      // Check category capacity for the requested period.
      var now = _clock.UtcNow;
      var conflict = await _conflicts.CheckAsync(
         carCategory: reservation.CarCategory,
         period: reservation.Period,
         ignoreReservationId: reservation.Id,
         ct: ct
      );

      if (conflict != ReservationConflict.None) {
         var error = Reservation.MapConflict(conflict);

         _logger.LogWarning(
            "ReservationUcConfirm rejected reservationId={id} conflict={conflict} errorCode={code}",
            reservationId, conflict, error.Code);

         return Result.Failure(error);
      }

      // Domain transition (pure, no I/O).
      var result = reservation.Confirm(now);
      if (result.IsFailure) {
         _logger.LogWarning(
            "ReservationUcConfirm rejected reservationId={id} errorCode={code}",
            reservationId, result.Error!.Code
         );
         return result;
      }

      // Persist updated state.
      await _unitOfWork.SaveAllChangesAsync("Reservation confirmed", ct);

      _logger.LogDebug("ReservationUcConfirm done reservationId={id}",
         reservationId);

      return Result.Success();
   }
}
