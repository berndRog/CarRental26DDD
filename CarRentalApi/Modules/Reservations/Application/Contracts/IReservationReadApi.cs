using CarRentalApi.Modules.Reservations.Application.Contracts.Dto;
namespace CarRentalApi.Modules.Reservations.Application.Contracts;

/// <summary>
/// Read-only facade of the Reservations bounded context.
/// Used by other bounded contexts (e.g. Rentals) for queries.
/// </summary>
public interface IReservationsReadApi {
   Task<ConfirmedReservationDto?> FindConfirmedByIdAsync(
      Guid reservationId,
      CancellationToken ct
   );
}