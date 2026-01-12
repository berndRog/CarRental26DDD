using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.Errors;
using CarRentalApi.Modules.Bookings.Infrastructure;
namespace CarRentalApi.Modules.Bookings.Application.UseCases;

public sealed class ReservationUcConfirm(
   IReservationRepository _repository,
   IUnitOfWork _unitOfWork,
   IReservationConflictPolicy _conflicts,
   ILogger<ReservationUcConfirm> _logger,
   IClock _clock
) {
   
   public async Task<Result> ExecuteAsync(Guid reservationId, CancellationToken ct) {

      // Load reservation (aggregate) from _repository.
      var reservation = await _repository.FindByIdAsync(reservationId, ct);
      if (reservation is null) {
         var failure = Result.Failure(ReservationErrors.NotFound);
         failure.LogIfFailure(_logger, "ReservationUcConfirm.NotFound", new { reservationId });
         return failure;
      }
      
      // Check conflicts: Category capacity for the requested period.
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

      // Apply Domain transition (pure)
      var result = reservation.Confirm(now);
      if (result.IsFailure) {
         var failure = Result.Failure(ReservationErrors.NotFound);
         failure.LogIfFailure(_logger, "ReservationUcConfirm.NotFound", new { reservationId });
         return failure;
      }

      // Persist updated state
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Reservation confirmed", ct);
      
      _logger.LogInformation(
         "ReservationUcConfirm done reservationId={reservationId} savedRows={rows}",
         reservation.Id, savedRows
      );

      return Result.Success();
   }
}
