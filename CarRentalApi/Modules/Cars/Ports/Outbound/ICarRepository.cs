using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Enums;
namespace CarRentalApi.Modules.Cars.Ports.Outbound;

/// <summary>
/// Repository interface for the Car aggregate.
///
/// Purpose:
/// - Provides access to Car aggregates for application use cases
/// - Encapsulates persistence concerns behind an abstraction
/// - Supports domain-specific queries required for fleet management
///
/// Architectural intent:
/// - Part of the Domain Layer (Repository abstraction)
/// - Implemented in the Infrastructure Layer (e.g. EF Core)
/// - Used exclusively by Application UseCases
///
/// Important:
/// - This repository works with domain aggregates (Car)
/// - It must NOT expose DTOs or projections
/// - It must NOT contain UI-optimized queries (paging, sorting)
/// </summary>
public interface ICarRepository {

   // ------------------------------------------------------------------
   // Queries (0..1)
   // ------------------------------------------------------------------
   /// <summary>
   /// Loads a car aggregate by its identifier.
   ///
   /// Tracking behavior:
   /// - The returned aggregate is tracked by the underlying DbContext
   ///   (required for state changes).
   ///
   /// Returns:
   /// - The car aggregate if it exists
   /// - Null if no car with the given id exists
   /// </summary>
   Task<Car?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   );

   /// <summary>
   /// Checks whether a car with the given license plate already exists.
   ///
   /// Business meaning:
   /// - Enforces uniqueness of license plates
   /// - Used during car creation and updates
   ///
   /// Returns:
   /// - True if a car with the given license plate exists
   /// - False otherwise
   /// </summary>
   Task<bool> ExistsLicensePlateAsync(
      string licensePlate,
      CancellationToken ct
   );

   /// <summary>
   /// Counts all cars in a given car category.
   ///
   /// Business meaning:
   /// - Used for fleet statistics and capacity calculations
   /// - Supports reservation and planning logic
   ///
   /// Returns:
   /// - The number of cars in the given category
   /// </summary>
   Task<int> CountCarsInCategoryAsync(
      CarCategory category,
      CancellationToken ct
   );

   // ------------------------------------------------------------------
   // Queries (0..n)
   // ------------------------------------------------------------------
   /// <summary>
   /// Selects cars by optional filter criteria.
   ///
   /// Business meaning:
   /// - Used by application use cases for fleet analysis
   /// - Supports filtering by category and/or operational status
   ///
   /// Technical notes:
   /// - This method returns tracked aggregates
   /// - Filtering parameters may be null to indicate "no filter"
   ///
   /// Returns:
   /// - A list of car aggregates matching the criteria
   /// - The list may be empty
   /// </summary>
   Task<IReadOnlyList<Car>> SelectByAsync(
      CarCategory? category,
      CarStatus? status,
      CancellationToken ct
   );

   // ------------------------------------------------------------------
   // Commands
   // ------------------------------------------------------------------
   /// <summary>
   /// Adds a new car aggregate to the persistence context.
   ///
   /// Technical notes:
   /// - This method is synchronous by design
   /// - The aggregate is attached to the DbContext and tracked
   /// - Persistence is finalized via UnitOfWork.SaveAllChangesAsync
   /// </summary>
   void Add(Car car);
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist ICarRepository?
 * -----------------------
 * ICarRepository ist das Repository-Interface für das
 * Car-Aggregate im Cars-/Fleet-Bounded-Context.
 *
 * Es kapselt den Zugriff auf Fahrzeuge und stellt sicher,
 * dass Application UseCases mit vollständigen Domain-Aggregaten
 * arbeiten können, ohne die Persistenztechnologie zu kennen.
 *
 * Was ist ICarRepository NICHT?
 * -----------------------------
 * - Kein ReadModel
 * - Kein UseCase
 * - Kein Service für UI-Abfragen
 *
 * Insbesondere:
 * - Es liefert keine DTOs oder ViewModels
 * - Es enthält kein Paging oder Sorting
 * - Es führt keine fachlichen Regeln aus
 *
 * Fachliche Bedeutung der Methoden:
 * ---------------------------------
 * - FindById:
 *   → Standardzugriff auf ein Fahrzeug zur Zustandsänderung
 *
 * - ExistsLicensePlateAsync:
 *   → Durchsetzung der fachlichen Regel:
 *     „Kennzeichen müssen eindeutig sein“
 *
 * - CountCarsInCategoryAsync:
 *   → Grundlage für Kapazitäts- und Planungslogik
 *
 * - SelectByAsync:
 *   → Aggregat-Zugriff für interne fachliche Auswertungen
 *     (nicht für UI-Listen!)
 *
 * Tracking vs. ReadModels:
 * ------------------------
 * - Repository-Methoden liefern TRACKING-Entitäten
 * - Nur so können Zustandsänderungen korrekt persistiert werden
 *
 * Für reine Anzeige- und Suchzwecke gilt:
 * → ReadModels (z.B. ICarReadModel) mit AsNoTracking()
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Zustandsänderungen:
 *   → Application UseCases (z.B. ICarUseCases)
 *
 * - Fachliche Regeln & Invarianten:
 *   → Car Aggregate (Domain Layer)
 *
 * - Persistenz-Details:
 *   → Infrastructure (EF Core)
 *
 *
 * Typische Implementierungsdetails:
 * ---------------------------------
 * - EF Core DbContext
 * - Change Tracking aktiviert
 * - Optimierte Queries für fachliche Fragestellungen
 *
 * Dadurch:
 * - bleibt das Domain Model konsistent
 * - sind fachliche Regeln korrekt durchsetzbar
 * - ist die DDD-Architektur sauber getrennt
 *
 * =====================================================================
 */
