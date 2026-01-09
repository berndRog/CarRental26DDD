using CarRentalApi.Modules.Reservations.Application.Contracts;
using CarRentalApi.Modules.Reservations.Application.Contracts.Dto;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
using CarRentalApi.Modules.Reservations.Domain.Enums;
using CarRentalApi.Modules.Reservations.Infrastructure;

namespace CarRentalApi.Modules.Reservations.Application.Services;

/// <summary>
/// Read-only application service (facade) for the Reservations bounded context.
/// Exposes only DTOs to other bounded contexts.
/// </summary>
public sealed class ReservationsReadService(
   IReservationRepository _reservationRepository
) : IReservationsReadApi
{
   public async Task<ConfirmedReservationDto?> FindConfirmedByIdAsync(
      Guid reservationId,
      CancellationToken ct
   ) {
      var reservation = await _reservationRepository.FindConfirmedByIdAsync(reservationId, ct);
      if (reservation is null)
         return null;

      return new ConfirmedReservationDto(
         ReservationId: reservation.Id,
         CustomerId: reservation.CustomerId,
         Category: reservation.CarCategory,   
         Start: reservation.Period.Start,
         End: reservation.Period.End
      );
   }
}