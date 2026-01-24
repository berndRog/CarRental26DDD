using CarRentalApi._2_Modules.Bookings._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi._4_BuildingBlocks._1_Ports.Inbound;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.Modules.Bookings.Domain;
namespace CarRentalApi._2_Modules.Bookings._2_Application.UseCases.Reservation;

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
      // Load aggregate from repository or query database
      var reservation = await _repository.FindByIdAsync(reservationId, ct);
      if (reservation is null) {
         var fail = Result.Failure(ReservationErrors.NotFound);
         fail.LogIfFailure(_logger, "ReservationUcCancel.NotFound",
            new { reservationId });
         return fail;
      }
      
      // Apply domain transisitions
      var now = _clock.UtcNow;
      var result = reservation.Cancel(now);
      if (result.IsFailure) {
         result.LogIfFailure(_logger, "ReservationUcCancel.DomainRejected",
            new { reservationId, now });
         return result;
      }

      // unit of work to save all changes to database
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Reservation cancelled", ct);
      
      _logger.LogInformation(
         "ReservationUcCancel done reservationId={reservationId} savedRows={rows}",
         reservation.Id, savedRows
      );
      
      return Result.Success();
   }
}