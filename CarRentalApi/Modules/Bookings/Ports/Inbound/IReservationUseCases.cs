using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
namespace CarRentalApi.Modules.Bookings;

/// <summary>
/// Application use cases for the Booking bounded context (Reservations).
///
/// Purpose:
/// - Encapsulates all state-changing operations related to reservations
/// - Coordinates domain aggregates, policies and repositories
/// - Acts as the application boundary for reservation commands
///
/// Scope:
/// - Create reservations
/// - Change reservation periods
/// - Confirm, cancel and expire reservations
///
/// Architectural intent:
/// - Part of the Application Layer
/// - Orchestrates domain logic but does not implement business rules itself
/// - Ensures transactional consistency via Unit of Work
///
/// Important:
/// - This interface represents COMMAND use cases (write side)
/// - It must NOT be used for read/query scenarios
/// - It must NOT be called from domain services or aggregates
///
/// Result policy:
/// - Success:
///   - Returns identifiers or completes without payload
/// - Failure:
///   - Returns domain-specific errors (validation, conflicts, not found)
/// </summary>
public interface IReservationUseCases {

   /// <summary>
   /// Creates a new reservation in Draft state.
   ///
   /// Business meaning:
   /// - Starts the reservation lifecycle
   /// - Captures the desired rental period and car category
   /// - Does NOT assign a concrete car
   ///
   /// Preconditions:
   /// - The customer must exist
   /// - The rental period must be valid (start < end, start in the future)
   ///
   /// Side effects:
   /// - Creates a new Reservation aggregate
   /// - Sets the initial status to Draft
   /// - Persists the reservation
   ///
   /// Returns:
   /// - Success(<see cref="Guid"/>) containing the reservation id
   /// - Invalid if input values are invalid
   /// </summary>
   Task<Result<Guid>> CreateAsync(
      Guid customerId,
      CarCategory carCategory,
      DateTimeOffset start,
      DateTimeOffset end,
      string? id = null,
      CancellationToken ct = default!
   );

   /// <summary>
   /// Changes the rental period of an existing reservation.
   ///
   /// Business meaning:
   /// - Adjusts the desired rental period before confirmation
   ///
   /// Preconditions:
   /// - The reservation must exist
   /// - The reservation must be in Draft or Confirmed state
   /// - The new period must be valid
   ///
   /// Side effects:
   /// - Updates the Reservation aggregate
   /// - Persists the modified state
   ///
   /// Returns:
   /// - Success if the period was changed
   /// - NotFound if the reservation does not exist
   /// - Conflict if the period cannot be changed in the current state
   /// - Invalid if the new period is invalid
   /// </summary>
   Task<Result> ChangePeriodAsync(
      Guid reservationId,
      DateTimeOffset newStart,
      DateTimeOffset newEnd,
      CancellationToken ct
   );

   /// <summary>
   /// Confirms a reservation.
   ///
   /// Business meaning:
   /// - Commits the reservation
   /// - Makes it eligible for pick-up (Rental creation)
   ///
   /// Preconditions:
   /// - The reservation must exist
   /// - The reservation must be in Draft state
   /// - Capacity and conflict checks must succeed
   ///
   /// Side effects:
   /// - Updates the reservation status to Confirmed
   /// - Persists the updated state
   ///
   /// Returns:
   /// - Success if the reservation was confirmed
   /// - NotFound if the reservation does not exist
   /// - Conflict if confirmation is not possible
   /// </summary>
   Task<Result> ConfirmAsync(
      Guid reservationId,
      CancellationToken ct = default
   );

   /// <summary>
   /// Cancels an existing reservation.
   ///
   /// Business meaning:
   /// - Aborts the reservation before pick-up
   ///
   /// Preconditions:
   /// - The reservation must exist
   /// - The reservation must not already be cancelled or expired
   ///
   /// Side effects:
   /// - Updates the reservation status to Cancelled
   /// - Persists the updated state
   ///
   /// Returns:
   /// - Success if the reservation was cancelled
   /// - NotFound if the reservation does not exist
   /// - Conflict if cancellation is not allowed
   /// </summary>
   Task<Result> CancelAsync(
      Guid reservationId,
      CancellationToken ct = default
   );

   /// <summary>
   /// Expires outdated reservations automatically.
   ///
   /// Business meaning:
   /// - Cleans up reservations that were never confirmed
   /// - Enforces time-based business rules
   ///
   /// Preconditions:
   /// - None (system-triggered use case)
   ///
   /// Side effects:
   /// - Transitions eligible reservations to Expired state
   /// - Persists all changes
   ///
   /// Returns:
   /// - Success(<see cref="int"/>) containing the number of expired reservations
   /// </summary>
   Task<Result<int>> ExpireAsync(
      CancellationToken ct = default
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Hinweise
 * =====================================================================
 *
 * Was ist IReservationUseCases?
 * ------------------------------
 * IReservationUseCases definiert die fachlichen Anwendungsfälle
 * (Use Cases) für den Booking-/Reservations-Bounded-Context.
 *
 * Es handelt sich um die WRITE-Seite des Systems:
 * - Erzeugen
 * - Ändern
 * - Bestätigen
 * - Abbrechen
 * - Automatisches Ablaufen (Expire)
 *
 *
 * Was ist IReservationUseCases NICHT?
 * -----------------------------------
 * - Kein ReadModel (keine Abfragen, keine Listen)
 * - Kein Repository (keine Persistenzdetails)
 * - Kein Domain Service (keine fachlichen Regeln im Application Layer)
 * - Kein Aggregate (keine Zustandsdatenhaltung)
 *
 *
 * Fachliche Einordnung der Use Cases:
 * ----------------------------------
 * - Create:
 *   - erzeugt eine Draft-Reservation
 *
 * - ChangePeriod:
 *   - erlaubt zeitliche Anpassungen vor dem eigentlichen Pick-up
 *
 * - Confirm:
 *   - verbindliche Buchung
 *   - Voraussetzung für Pick-up / Rental
 *
 * - Cancel:
 *   - expliziter Abbruch durch Benutzer oder System
 *
 * - Expire:
 *   - systemgesteuerter Prozess (z.B. per Job)
 *   - verhindert „hängende“ Draft-Reservations
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Lesen / Anzeigen:
 *   → IReservationReadModel
 *
 * - Fachliche Regeln & Invarianten:
 *   → Reservation Aggregate (Domain Layer)
 *
 * - Persistenz:
 *   → ReservationRepository (Infrastructure)
 *
 *
 * Warum Result<T>?
 * ----------------
 * - Einheitliches Fehler- und Erfolgshandling
 * - Klare Abbildung fachlicher Fehler:
 *   - NotFound
 *   - Conflict
 *   - Invalid
 *
 *
 * Typische Aufrufer:
 * ------------------
 * - API-Controller
 * - Application Services
 *
 * =====================================================================
 */
