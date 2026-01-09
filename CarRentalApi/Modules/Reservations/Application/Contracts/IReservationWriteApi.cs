using CarRentalApi.BuildingBlocks;
namespace CarRentalApi.Modules.Reservations.Application.Contracts;

/// <summary>
/// Command facade of the Reservations bounded context.
/// Exposes only valid state transitions.
/// </summary>
public interface IReservationsWriteApi {
   /// <summary>
   /// Marks a confirmed reservation as Used after pick-up.
   /// Must be idempotent.
   /// </summary>
   Task<Result> MarkAsRentedAsync(Guid reservationId, CancellationToken ct);
}