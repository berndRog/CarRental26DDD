using CarRentalApi.BuildingBlocks;
namespace CarRentalApi.Modules.Rentals.Application.Contracts;

/// <summary>
/// Read-only facade of the Rentals bounded context.
///
/// This API exposes query operations for rental data
/// without allowing any modifications.
/// 
/// Typical usage:
/// - Used by other bounded contexts (e.g. Reservations, Cars)
/// - Used by application services to correlate reservations and rentals
/// </summary>
public interface IRentalsReadApi {

   /// <summary>
   /// Finds the rental identifier for a given reservation.
   ///
   /// Business meaning:
   /// - A reservation may result in at most one rental
   /// - A rental is created when the customer picks up the car
   /// - Before pick-up, no rental exists for the reservation
   ///
   /// Typical use cases:
   /// - Determine whether a reservation has already been picked up
   /// - Prevent duplicate rental creation for the same reservation
   ///
   /// Returns:
   /// - Success with the rental id if a rental exists for the reservation
   /// - Success with null if no rental has been created yet
   /// - Failure if the reservation id is invalid or the query cannot be executed
   /// </summary>
   Task<Result<Guid?>> FindRentalIdByReservationIdAsync(
      Guid reservationId,
      CancellationToken ct
   );
}
