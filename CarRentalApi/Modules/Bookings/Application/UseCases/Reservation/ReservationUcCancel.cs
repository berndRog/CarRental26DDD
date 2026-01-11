using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Bookings.Domain.Errors;
using CarRentalApi.Modules.Bookings.Infrastructure;
namespace CarRentalApi.Modules.Bookings.Application.UseCases;

public sealed class ReservationUcCancel(
   IReservationRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<ReservationUcCancel> _logger,
   IClock _clock
) {

   public async Task<Result> ExecuteAsync(
      Guid reservationId, 
      CancellationToken ct
   ) {
      // fetch from repository or query database
      var reservation = await _repository.FindByIdAsync(reservationId, ct);
      if (reservation is null) {
         _logger.LogWarning(
            "ReservationUcCancel rejected reservationId={ReservationId} not found", reservationId);
         return Result.Failure(ReservationErrors.NotFound);
      }
      
      // domain model operation
      var now = _clock.UtcNow;
      var result = reservation.Cancel(now);
      if (result.IsFailure) {
         _logger.LogWarning(
            "ReservationUcCancel rejected reservationId={ReservationId} message={message}",
            reservationId, result.Error.Message);
         return result;
      }

      // unit of work to save all changes to database
      var saved = await _unitOfWork.SaveAllChangesAsync("Reservation cancelled", ct);
      return Result.Success();
   }
}