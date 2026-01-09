
using CarRentalApi.BuildingBlocks;

namespace CarRentalApi.Modules.Rentals.Application.Contracts;

/// <summary>
/// Command facade of the Rentals bounded context.
///
/// This API exposes write operations that create or modify rentals.
/// </summary>
public interface IRentalsWriteApi {

   /// <summary>
   /// Creates a rental at pick-up time based on an existing confirmed reservation.
   ///
   /// Business meaning:
   /// - The customer arrives at the counter
   /// - An employee selects an existing reservation
   /// - A suitable car is assigned to the rental
   /// - The rental is created and becomes active
   ///
   /// Expected rules:
   /// - Reservation must exist
   /// - Reservation must be confirmed
   /// - No rental must already exist for the reservation
   /// - A suitable car must be available
   ///
   /// Returns:
   /// - Success with the newly created rental identifier
   /// - Failure if any business rule is violated
   /// </summary>
   Task<Result<Guid>> StartFromReservationAsync(
      Guid reservationId,
      CancellationToken ct
   );

   /// <summary>
   /// Closes an active rental at return time.
   ///
   /// Business meaning:
   /// - The customer returns the car
   /// - The rental is closed and becomes inactive
   /// - The car becomes available again (unless sent to maintenance)
   ///
   /// Expected rules:
   /// - Rental must exist
   /// - Rental must currently be active
   ///
   /// Returns:
   /// - Success if the rental was successfully closed
   /// - Failure if the rental does not exist or cannot be closed
   /// </summary>
   Task<Result> CloseAsync(
      Guid rentalId,
      CancellationToken ct
   );
}
