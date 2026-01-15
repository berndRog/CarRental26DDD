using CarRentalApi.Modules.Rentals.Domain.Aggregates;

namespace CarRentalApi.Modules.Bookings.Domain;

/// <summary>
/// Repository interface for the Rental aggregate.
///
/// Purpose:
/// - Provides access to Rental aggregates for application use cases
/// - Encapsulates persistence concerns behind an abstraction
/// - Ensures aggregates are loaded and tracked correctly
///
/// Architectural intent:
/// - Part of the Domain Layer (Repository abstraction)
/// - Implemented in the Infrastructure Layer (e.g. EF Core)
/// - Used exclusively by Application UseCases
///
/// Important:
/// - This repository works with domain aggregates (Rental)
/// - It must NOT expose DTOs or projections
/// - It must NOT contain query-specific optimizations for UI
/// </summary>
public interface IRentalRepository {

   // ------------------------------------------------------------------
   // Queries (0..1)
   // ------------------------------------------------------------------
   /// <summary>
   /// Loads a rental aggregate by its identifier.
   ///
   /// Tracking behavior:
   /// - The returned aggregate is tracked by the underlying DbContext
   ///   (required for state changes).
   ///
   /// Returns:
   /// - The rental aggregate if it exists
   /// - Null if no rental with the given id exists
   /// </summary>
   Task<Rental?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   );

   /// <summary>
   /// Loads a rental aggregate by its associated reservation id.
   ///
   /// Business meaning:
   /// - Used to check whether a reservation has already been picked up
   ///
   /// Tracking behavior:
   /// - The returned aggregate is tracked by the underlying DbContext
   ///
   /// Returns:
   /// - The rental aggregate if it exists
   /// - Null if the reservation has not been picked up yet
   /// </summary>
   Task<Rental?> FindByReservationIdAsync(
      Guid reservationId,
      CancellationToken ct
   );
   
   /// <summary>
   /// Checks whether a rental already exists for the given reservation.
   ///
   /// Business meaning:
   /// - Used during the pick-up workflow to ensure that
   ///   a reservation is not picked up more than once
   ///
   /// Technical purpose:
   /// - Lightweight existence check without loading the aggregate
   /// - Used as a guard against duplicate or concurrent pick-up attempts
   ///
   /// Tracking behavior:
   /// - No aggregate is loaded
   /// - No change tracking is required
   ///
   /// Returns:
   /// - True if a rental already exists for the reservation
   /// - False if the reservation has not been picked up yet
   /// </summary>
   Task<bool> ExistsForReservationAsync(
      Guid reservationId,
      CancellationToken ct
   );
   
   // ------------------------------------------------------------------
   // Queries (0..n)
   // ------------------------------------------------------------------
   /// <summary>
   /// Returns all rentals of a given customer.
   ///
   /// Business meaning:
   /// - Used for customer history and overview screens
   ///
   /// Returns:
   /// - A list of rental aggregates
   /// - The list may be empty if the customer has no rentals
   /// </summary>
   Task<IReadOnlyList<Rental>> SelectByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct
   );

   /// <summary>
   /// Returns all rentals associated with a specific car.
   ///
   /// Business meaning:
   /// - Used for fleet management and car history
   ///
   /// Returns:
   /// - A list of rental aggregates
   /// - The list may be empty if the car has no rentals
   /// </summary>
   Task<IReadOnlyList<Rental>> SelectByCarIdAsync(
      Guid carId,
      CancellationToken ct
   );

   // ------------------------------------------------------------------
   // Commands
   // ------------------------------------------------------------------
   /// <summary>
   /// Adds a new rental aggregate to the persistence context.
   ///
   /// Technical notes:
   /// - This method is synchronous by design
   /// - The aggregate is attached to the DbContext and tracked
   /// - Persistence is finalized via UnitOfWork.SaveAllChangesAsync
   /// </summary>
   void Add(Rental rental);
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist IRentalRepository?
 * --------------------------
 * IRentalRepository ist das Repository-Interface für das
 * Rental-Aggregate.
 *
 * Es stellt eine Abstraktion über den Datenzugriff dar und erlaubt
 * Application UseCases, mit Rental-Aggregaten zu arbeiten,
 * ohne die Persistenztechnologie zu kennen.
 *
 *
 * Was ist IRentalRepository NICHT?
 * --------------------------------
 * - Kein ReadModel
 * - Kein UseCase
 * - Kein Service für UI-Abfragen
 *
 * Insbesondere:
 * - Es liefert keine DTOs
 * - Es führt keine Projektionen oder Paging durch
 * - Es enthält keine fachlichen Regeln
 *
 *
 * Warum gibt es getrennte Query-Methoden?
 * ----------------------------------------
 * Die Methoden spiegeln fachliche Zugriffsmuster wider:
 * - FindById:
 *   → klassischer Aggregate-Zugriff für Zustandsänderungen
 *
 * - FindByReservationId:
 *   → Prüfung, ob aus einer Reservation bereits ein Rental wurde
 *
 * - SelectByCustomerId / SelectByCarId:
 *   → Historien- und Verwaltungsfälle
 *
 * Wichtig:
 * - Diese Methoden sind für UseCases gedacht,
 *   nicht für UI-Listen oder Suchmasken
 *
 *
 * Tracking vs. NoTracking:
 * ------------------------
 * - Repository-Methoden liefern TRACKING-Entitäten
 * - Nur so können Zustandsänderungen am Aggregate
 *   korrekt persistiert werden
 *
 * Für reine Leseabfragen gilt:
 * → ReadModels (z.B. IRentalReadModel) verwenden AsNoTracking()
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Schreiben / Zustandsänderungen:
 *   → Application UseCases
 *
 * - Lesen / UI-Abfragen:
 *   → ReadModels
 *
 * - Persistenz-Details:
 *   → Infrastructure (EF Core, SQL, etc.)
 *
 *
 * Typische Implementierungsdetails:
 * ---------------------------------
 * - EF Core DbContext
 * - Change Tracking aktiviert
 * - Keine Optimierungen für UI (kein Paging, kein Sorting)
 *
 * Dadurch:
 * - bleibt das Domain Model konsistent
 * - sind Zustandsänderungen korrekt transaktional
 * - ist die Architektur sauber nach DDD getrennt
 *
 * =====================================================================
 */
