using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Rentals.Infrastructure; // IRentalRepository (Namespace ggf. anpassen)
using CarRentalApi.Modules.Reservations.Application.Contracts;
using CarRentalApi.Modules.Reservations.Domain.Errors;
using CarRentalApi.Modules.Reservations.Infrastructure;
using Microsoft.Extensions.Logging;

namespace CarRentalApi.Modules.Reservations.Application.Services;

public sealed class ReservationsWriteService(
   IReservationRepository _reservationRepository,
   IRentalRepository _rentalRepository,
   IUnitOfWork _unitOfWork,
   ILogger<ReservationsWriteService> _logger
) : IReservationsWriteApi
{
   public async Task<Result> MarkAsRentedAsync(
      Guid reservationId,
      CancellationToken ct
   ) {
      var reservation = await _reservationRepository.FindByIdAsync(reservationId, ct);
      if (reservation is null)
         return Result.Failure(ReservationErrors.NotFound);

      // Find rental created at pick-up for this reservation
      var rental = await _rentalRepository.FindByReservationIdAsync(reservationId, ct);

      // No rental exists -> cannot mark reservation as "used"
      // (we must use existing errors only)
      if (rental is null)
         return Result.Failure(ReservationErrors.InvalidStatusTransition);

      // Idempotency + invariant inside Reservation aggregate
      var result = reservation.AssignRental(rental.Id);
      if (result.IsFailure)
         return result;

      await _unitOfWork.SaveAllChangesAsync("Mark as Rented",ct);

      _logger.LogInformation(
         "Reservation {reservationId} marked as rented (RentalId={rentalId})",
         reservationId,
         rental.Id
      );

      return Result.Success();
   }
}
