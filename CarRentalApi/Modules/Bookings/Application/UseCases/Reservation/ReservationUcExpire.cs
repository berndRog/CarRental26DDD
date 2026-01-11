using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.BuildingBlocks.Utils;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Bookings.Infrastructure;
namespace CarRentalApi.Modules.Bookings.Application.UseCases;

public sealed class ReservationUcExpire(
   IReservationRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<ReservationUcExpire> _logger,
   IClock _clock
) {
   
   public async Task<Result<int>> ExecuteAsync(CancellationToken ct) {
      
      DateTimeOffset now = _clock.UtcNow;
      _logger.LogInformation("ReservationUcExpire start nowUtc={now}", now.ToDateTimeString());

      // fetch drafts to expire from repository or database
      var drafts = await _repository.SelectDraftsToExpireAsync(now, ct);
      _logger.LogInformation("ReservationUcExpire found draftsToExpire={count}", drafts.Count);

      // domain logic to expire each draft reservation
      var expiredCount = 0;
      foreach (var reservation in drafts) {
         var result = reservation.Expire(now);
         if (result.IsFailure) {
            _logger.LogWarning(
               "ReservationUcExpire skip reservationId={reservationId} errorCode={code} message={message}",
               reservation.Id, result.Error.Code, result.Error.Message);
            continue;
         }
         expiredCount++;
      }

      // unit of work to save all changes to database
      var saved = await _unitOfWork.SaveAllChangesAsync("Expired reservation",ct);
      _logger.LogInformation(
         "ReservationUcExpire done expiredCount={expiredCount} savedRows={saved}",
         expiredCount, saved);

      return Result<int>.Success(expiredCount);
   }
}
