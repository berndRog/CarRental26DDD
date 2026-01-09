using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Reservations.Domain.Errors;
using CarRentalApi.Modules.Reservations.Infrastructure;
namespace CarRentalApi.Modules.Reservations.Application.UseCases;

public sealed class ReservationUcChangePeriod(
   IReservationRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<ReservationUcChangePeriod> _logger,
   IClock _clock
) {

   public async Task<Result> ExecuteAsync(
      Guid reservationId,
      DateTimeOffset start,
      DateTimeOffset end,
      CancellationToken ct
   ) {
    
      // fetch from repository
      var reservation = await _repository.FindByIdAsync(reservationId, ct);
      if (reservation is null) {
         _logger.LogWarning(
            "ReservationUcChangePeriod rejected reservationId={Id} not found", 
            reservationId);
         return Result.Failure(ReservationErrors.NotFound);
      }
      
      // domain model operation
      // Customers may only create reservation in the future (start must be > now).
      var now = _clock.UtcNow;
      if (start <= now)
         return Result.Failure(ReservationErrors.StartDateInPast);
      
      // create new Period value object and change reservation period
      var result = reservation.ChangePeriod(start, end);
      if (result.IsFailure) {
         _logger.LogWarning(
            "ReservationUcChangePeriod rejected reservationId={Id} message={message}",
            reservationId, result.Error!.Message);
         return result;
      }
      
      // unit of work to save changes
      await _unitOfWork.SaveAllChangesAsync("Reservation period changed", ct);
      return Result.Success();
   }
}
