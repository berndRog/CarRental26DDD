using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
using CarRentalApi.Modules.Reservations.Domain.Errors;
using CarRentalApi.Modules.Reservations.Infrastructure;
namespace CarRentalApi.Modules.Reservations.Application.UseCases;

public sealed class ReservationUcCreate(
   IReservationRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<ReservationUcCreate> _logger,
   IClock _clock
) {
   
   public async Task<Result<Reservation>> ExecuteAsync(
      Guid customerId,
      CarCategory carCategory,
      DateTimeOffset start,
      DateTimeOffset end,
      string? id,
      CancellationToken ct
   ) {
      _logger.LogInformation(
         "ReservationUcCreateDraft start customerId={customerId} carCategory={cat} start={start} end={end}",
         customerId, carCategory, start, end
      );

      // Use-case rule:
      // Customers may only create reservation in the future (start must be > now).
      var now = _clock.UtcNow;
      if (start <= now)
         return Result<Reservation>.Failure(ReservationErrors.StartDateInPast);

      // Domain factory: enforces domain invariants (e.g., end > start).
      var result = Reservation.Create(
         customerId: customerId,
         carCategory: carCategory,
         start: start,
         end: end,
         createdAt: now,
         id: id
      );

      if (result.IsFailure) {
         _logger.LogWarning(
            "ReservationUcCreateDraft rejected errorCode={code} message={message}",
            result.Error.Code, result.Error.Message);
         return Result<Reservation>.Failure(result.Error);
      }

      var reservation = result.Value!;

      // Add the new reservation to the _repository (tracked by EF).
      _repository.Add(reservation);

      // Persist changes.
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Reservation draft added", ct);

      _logger.LogInformation(
         "ReservationUcCreateDraft done reservationId={reservationId} savedRows={rows}",
         reservation.Id, savedRows
      );

      return Result<Reservation>.Success(reservation);
   }
}

