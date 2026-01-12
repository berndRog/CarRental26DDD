using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;

namespace CarRentalApi.Modules.Bookings.Domain;

/// <summary>
/// Repository interface for the Reservation aggregate.
///
/// Purpose:
/// - Provides access to Reservation aggregates for application use cases
/// - Encapsulates persistence concerns behind an abstraction
/// - Supports domain-specific query patterns required by the booking workflow
///
/// Architectural intent:
/// - Part of the Domain Layer (Repository abstraction)
/// - Implemented in the Infrastructure Layer (e.g. EF Core)
/// - Used exclusively by Application UseCases
///
/// Important:
/// - This repository works with domain aggregates (Reservation)
/// - It must NOT expose DTOs or projections
/// - It must NOT contain UI-optimized queries (paging, sorting)
/// </summary>
public interface IReservationRepository {

   // ------------------------------------------------------------------
   // Queries (0..1)
   // ------------------------------------------------------------------
   /// <summary>
   /// Loads a reservation aggregate by its identifier.
   ///
   /// Tracking behavior:
   /// - The returned aggregate is tracked by the underlying DbContext
   ///   (required for state changes).
   ///
   /// Returns:
   /// - The reservation aggregate if it exists
   /// - Null if no reservation with the given id exists
   /// </summary>
   Task<Reservation?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   );

   /// <summary>
   /// Loads a confirmed reservation aggregate by its identifier.
   ///
   /// Business meaning:
   /// - Used when only confirmed reservations are relevant
   ///   (e.g. for pick-up workflows or cross-BC checks)
   ///
   /// Tracking behavior:
   /// - The returned aggregate is tracked by the underlying DbContext
   ///
   /// Returns:
   /// - The confirmed reservation aggregate if it exists
   /// - Null if the reservation does not exist or is not confirmed
   /// </summary>
   Task<Reservation?> FindConfirmedByIdAsync(
      Guid id,
      CancellationToken ct
   );

   /// <summary>
   /// Counts confirmed reservations that overlap with the given period
   /// for a specific car category.
   ///
   /// Business meaning:
   /// - Used to enforce capacity constraints per car category
   /// - Supports conflict detection during reservation confirmation
   ///
   /// Technical notes:
   /// - The reservation with <paramref name="ignoreReservationId"/>
   ///   must be excluded from the count (self-ignore).
   ///
   /// Returns:
   /// - The number of overlapping confirmed reservations
   /// </summary>
   Task<int> CountConfirmedOverlappingAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      Guid ignoreReservationId,
      CancellationToken ct
   );

   // ------------------------------------------------------------------
   // Queries (0..n)
   // ------------------------------------------------------------------
   /// <summary>
   /// Returns all draft reservations that should be expired.
   ///
   /// Business meaning:
   /// - Used by background jobs or scheduled use cases
   /// - Identifies reservations that were never confirmed
   ///   within the allowed time window
   ///
   /// Returns:
   /// - A list of draft reservations eligible for expiration
   /// - The list may be empty
   /// </summary>
   Task<IReadOnlyList<Reservation>> SelectDraftsToExpireAsync(
      DateTimeOffset now,
      CancellationToken ct
   );

   // ------------------------------------------------------------------
   // Commands
   // ------------------------------------------------------------------
   /// <summary>
   /// Adds a new reservation aggregate to the persistence context.
   ///
   /// Technical notes:
   /// - This method is synchronous by design
   /// - The aggregate is attached to the DbContext and tracked
   /// - Persistence is finalized via UnitOfWork.SaveAllChangesAsync
   /// </summary>
   void Add(Reservation reservation);
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist IReservationRepository?
 * --------------------------------
 * IReservationRepository ist das Repository-Interface für das
 * Reservation-Aggregate im Booking-Bounded-Context.
 *
 * Es kapselt den Zugriff auf Reservierungen und stellt sicher,
 * dass Application UseCases mit vollständigen Domain-Aggregaten
 * arbeiten können.
 *
 *
 * Was ist IReservationRepository NICHT?
 * -------------------------------------
 * - Kein ReadModel
 * - Kein UseCase
 * - Kein Service für UI-Abfragen
 *
 * Insbesondere:
 * - Es liefert keine DTOs oder ViewModels
 * - Es enthält kein Paging, kein Sorting
 * - Es implementiert keine Geschäftsregeln
 *
 *
 * Warum gibt es diese speziellen Query-Methoden?
 * -----------------------------------------------
 * Die Methoden spiegeln fachliche Anforderungen wider:
 *
 * - FindById:
 *   → Standardzugriff auf ein Aggregate zur Zustandsänderung
 *
 * - FindConfirmedById:
 *   → Sicherheitsmechanismus, wenn nur bestätigte Reservierungen
 *     fachlich relevant sind (z.B. Pick-up)
 *
 * - CountConfirmedOverlappingAsync:
 *   → Zentrale Grundlage für Kapazitäts- und Konfliktprüfungen
 *     pro CarCategory
 *
 * - SelectDraftsToExpireAsync:
 *   → Unterstützung für systemgesteuerte Abläufe (Expire-Job)
 *
 *
 * Tracking vs. ReadModels:
 * ------------------------
 * - Repository-Methoden liefern TRACKING-Entitäten
 * - Nur so können Zustandsänderungen korrekt gespeichert werden
 *
 * Für reine Anzeige- oder Suchzwecke gilt:
 * → ReadModels (z.B. IReservationReadModel) mit AsNoTracking()
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Zustandsänderungen:
 *   → Application UseCases
 *
 * - Fachliche Regeln & Invarianten:
 *   → Reservation Aggregate (Domain Layer)
 *
 * - Persistenz-Details:
 *   → Infrastructure (EF Core)
 *
 *
 * Typische Implementierungsdetails:
 * ---------------------------------
 * - EF Core DbContext
 * - Change Tracking aktiviert
 * - Optimierte Queries für fachliche Regeln (Counts, Overlaps)
 *
 * Dadurch:
 * - bleibt das Domain Model konsistent
 * - sind fachliche Regeln korrekt durchsetzbar
 * - ist die DDD-Architektur sauber getrennt
 *
 * =====================================================================
 */
