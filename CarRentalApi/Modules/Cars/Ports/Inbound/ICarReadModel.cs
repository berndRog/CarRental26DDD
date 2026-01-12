using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
namespace CarRentalApi.Modules.Cars.Application.ReadModel;

/// <summary>
/// Read-only query model for the Cars bounded context.
///
/// Purpose:
/// - Used by API controllers and UI-facing endpoints
/// - Provides flexible read access via filtering, paging and sorting
/// - Returns projection-based read models (DTOs / ViewModels)
/// - Does NOT expose domain aggregates
/// - Does NOT modify state
///
/// Architectural intent:
/// - Serves HTTP/API read endpoints only
/// - Optimized for query use-cases and UI needs
/// - Allows efficient database projections and indexing
///
/// Important:
/// - This is NOT a bounded-context facade
/// - Other bounded contexts must use ICarsReadApi (Contracts)
/// - This interface must not be used by domain or application services
///
/// Result policy:
/// - Success:
///   - Single-item queries return the requested projection
///   - List queries return data, including empty result sets
/// - NotFound:
///   - Returned for single-item queries when the entity does not exist
/// - Invalid:
///   - Returned when input parameters are invalid
///     (e.g. paging or sorting values out of range)
/// </summary>
public interface ICarReadModel {

   /// <summary>
   /// Returns detailed information about a single car.
   ///
   /// Business meaning:
   /// - Used by detail views (e.g. admin UI, backoffice)
   /// - Shows the current state of a car including:
   ///   - category
   ///   - license plate
   ///   - maintenance and retirement status
   ///
   /// Technical notes:
   /// - Read-only projection
   /// - No domain logic is executed
   ///
   /// Returns:
   /// - Success(CarDetails) if the car exists
   /// - NotFound if the car does not exist
   /// </summary>
   Task<Result<CarDetails>> FindByIdAsync(
      Guid carId,
      CancellationToken ct
   );

   /// <summary>
   /// Searches cars using flexible filter, paging and sorting parameters.
   ///
   /// Business meaning:
   /// - Used by list views, search screens and admin dashboards
   /// - Supports combination of multiple filter criteria
   /// - Paging and sorting are always applied
   ///
   /// Technical notes:
   /// - All list-based queries are expressed via this method
   /// - No specialized list endpoints exist (e.g. "by category", "in maintenance")
   /// - Filtering, paging and sorting are translated directly into database queries
   ///
   /// Returns:
   /// - Success(PagedResult&lt;CarListItem&gt;)
   ///   - Items may be empty if no cars match the criteria
   /// - Invalid if paging or sorting parameters are invalid
   /// </summary>
   Task<Result<PagedResult<CarListItem>>> SearchAsync(
      CarSearchFilter filter,
      PageRequest page,
      SortRequest sort,
      CancellationToken ct
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist ICarReadModel?
 * ----------------------
 * ICarReadModel ist ein ReadModel (Query Model) für den
 * Cars-/Fleet-Bounded-Context.
 *
 * Es stellt ausschließlich Lesezugriffe auf Fahrzeugdaten bereit
 * und liefert dafür Projektionen (DTOs), keine Domain-Aggregates.
 *
 * Typische Anwendungsfälle:
 * - Fahrzeug-Detailansicht (Admin / Backoffice)
 * - Fahrzeuglisten und Suchmasken
 * - Verwaltungs- und Übersichtsansichten
 *
 *
 * Was ist ICarReadModel NICHT?
 * ----------------------------
 * - Kein UseCase (keine Zustandsänderungen)
 * - Kein Repository (keine Persistenzverantwortung)
 * - Kein Domain Service
 * - Kein Bounded-Context-Facade
 *
 * Insbesondere:
 * - Es ändert keinen Fahrzeugstatus
 * - Es führt keine fachlichen Regeln aus
 * - Es kennt keine Invarianten des Car-Aggregates
 *
 *
 * Warum ein eigenes ReadModel?
 * -----------------------------
 * - Trennung von Lesen und Schreiben (CQRS)
 * - Optimierung für UI- und Suchanforderungen
 * - Vermeidung von unnötigem EF-Core-Tracking
 * - Keine Leaks von Domain-Objekten nach außen
 *
 *
 * Warum gibt es nur eine SearchAsync-Methode?
 * --------------------------------------------
 * Fachliche und technische Entscheidung:
 * - Alle Listen- und Suchanforderungen werden über
 *   Filter + Paging + Sorting abgebildet
 * - Keine spezialisierten Methoden wie:
 *   - FindByCategory
 *   - FindInMaintenance
 *
 * Vorteile:
 * - Einheitlicher API-Zugriff
 * - Einfachere Erweiterbarkeit
 * - Klare Kontrolle erlaubter Filter- und Sortierfelder
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Zustandsänderungen (Create, Maintenance, Retire):
 *   → ICarUseCases (Application Layer)
 *
 * - Fachliche Regeln & Zustandsautomaten:
 *   → Car Aggregate (Domain Layer)
 *
 * - Persistenz / EF Core:
 *   → CarRepository (Infrastructure)
 *
 * - BC-übergreifende Abfragen:
 *   → ICarsReadApi (Contracts)
 *
 *
 * Typische Implementierungsdetails:
 * ---------------------------------
 * - EF Core mit AsNoTracking()
 * - Projektionen via Select(...)
 * - Keine Include(...)
 * - Nutzung von Indizes für Filter- und Sortierfelder
 *
 * Dadurch:
 * - hohe Performance
 * - saubere BC-Grenzen
 * - klare CQRS-Struktur
 *
 * =====================================================================
 */
