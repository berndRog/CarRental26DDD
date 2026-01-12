using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Domain.Errors;
using CarRentalApi.Modules.Bookings.Infrastructure;
namespace CarRentalApi.Modules.Bookings.Application.UseCases;

public sealed class ReservationUcCreate(
   IReservationRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<ReservationUcCreate> _logger,
   IClock _clock
) {
   
   public async Task<Result<Guid>> ExecuteAsync(
      Guid customerId,
      CarCategory carCategory,
      DateTimeOffset start,
      DateTimeOffset end,
      string? id,
      CancellationToken ct
   ) {

      // Use-case rule:
      // Customers may only create reservation in the future (start must be > now).
      var now = _clock.UtcNow;
      if (start <= now) {
         var failure = Result<Guid>.Failure(ReservationErrors.StartDateInPast);
         failure.LogIfFailure(_logger, "ReservationUcCreate.StartDateInPast",
            new { customerId, carCategory, start, now });
         return failure;
      }

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
         result.LogIfFailure(_logger, "ReservationUcCreate.DomainRejected",
            new { customerId, carCategory, start, end });
         return Result<Guid>.Failure(result.Error);
      }
      
      // Add the new reservation to the _repository (tracked by EF).
      var reservation = result.Value!;
      _repository.Add(reservation);

      // Persist changes.
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Reservation draft added", ct);

      _logger.LogInformation(
         "ReservationUcCreateDraft done reservationId={reservationId} savedRows={rows}",
         reservation.Id, savedRows
      );

      return Result<Guid>.Success(reservation.Id);
   }
}

