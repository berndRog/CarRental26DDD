using CarRentalApi.BuildingBlocks;
using CarRentalApi.Modules.Rentals.Domain.Enums;

namespace CarRentalApi.Modules.Rentals.Application.UseCases;

/// <summary>
/// Application use cases for the Rentals bounded context.
///
/// Purpose:
/// - Encapsulates all write operations related to the rental lifecycle
/// - Coordinates domain aggregates, policies and repositories
/// - Acts as the application boundary for state-changing operations
///
/// Scope:
/// - Pick-up of a confirmed reservation
/// - Return of an active rental
///
/// Architectural intent:
/// - Part of the Application Layer
/// - Orchestrates domain logic, but does not contain business rules itself
/// - Enforces transactional consistency via Unit of Work
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
public interface IRentalUseCases {

   /// <summary>
   /// Performs the pick-up of a car for a confirmed reservation
   /// and creates a new rental.
   ///
   /// Business meaning:
   /// - Marks the transition from Reservation → Rental
   /// - Assigns a concrete car to the rental
   /// - Captures the initial state of the vehicle at pick-up
   ///
   /// Preconditions:
   /// - The reservation must exist
   /// - The reservation must be in status "Confirmed"
   /// - No rental must already exist for the reservation
   ///
   /// Side effects:
   /// - Creates a new Rental aggregate
   /// - Sets the rental status to Active
   /// - Persists the rental in the database
   ///
   /// Returns:
   /// - Success(<see cref="Guid"/>) containing the newly created rental id
   /// - NotFound if the reservation does not exist
   /// - Conflict if the reservation has already been picked up
   /// - Invalid if input values are out of range
   /// </summary>
   Task<Result<Guid>> PickupAsync(
      Guid reservationId,
      RentalFuelLevel fuelOut,
      int kmOut,
      CancellationToken ct
   );

   /// <summary>
   /// Performs the return of an active rental and closes it.
   ///
   /// Business meaning:
   /// - Marks the end of the rental lifecycle
   /// - Captures the final state of the vehicle at return
   ///
   /// Preconditions:
   /// - The rental must exist
   /// - The rental must be in status "Active"
   ///
   /// Side effects:
   /// - Updates the Rental aggregate
   /// - Sets the rental status to Returned
   /// - Persists the updated state in the database
   ///
   /// Returns:
   /// - Success if the rental was successfully returned
   /// - NotFound if the rental does not exist
   /// - Conflict if the rental is already returned
   /// - Invalid if input values are out of range
   /// </summary>
   Task<Result> ReturnAsync(
      Guid rentalId,
      RentalFuelLevel fuelIn,
      int kmIn,
      CancellationToken ct
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist IRentalUseCases?
 * ------------------------
 * IRentalUseCases definiert die fachlichen Anwendungsfälle (Use Cases)
 * für den Rentals-Bounded-Context.
 *
 * Es handelt sich um die WRITE-Seite des Systems:
 * - zustandsverändernde Operationen
 * - keine Abfragen
 *
 *
 * Abgrenzung zu anderen Konzepten:
 * --------------------------------
 * - Read Models:
 *   → IRentalReadModel
 *
 * - Domain Model:
 *   → Rental Aggregate (Status, Invarianten, Übergänge)
 *
 * - Repository:
 *   → Persistenz des Aggregates
 *
 *
 * Warum zwei getrennte Use Cases?
 * --------------------------------
 * - Pick-up:
 *   - Übergang von Reservation → Rental
 *   - Erzeugung eines neuen Aggregates
 *
 * - Return:
 *   - Abschluss eines bestehenden Aggregates
 *   - Statuswechsel Active → Returned
 *
 *
 * Warum Rückgabewerte vom Typ Result?
 * ----------------------------------
 * - Einheitliche Fehlerbehandlung
 * - Klare Abbildung fachlicher Fehler:
 *   - NotFound
 *   - Conflict
 *   - Invalid
 *
 *
 * Wer darf diese Use Cases aufrufen?
 * ----------------------------------
 * - API-Controller
 * - ggf. Application Services
 *
 * Wer darf sie NICHT aufrufen?
 * -----------------------------
 * - Domain Services
 * - Aggregates
 * - Read Models
 *
 * =====================================================================
 */
