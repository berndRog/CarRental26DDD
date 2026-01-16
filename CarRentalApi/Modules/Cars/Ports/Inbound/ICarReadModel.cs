using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Bookings.Application.Pricing.Dto;
using CarRentalApi.Modules.Cars.Application.Dto;
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
/// - Other bounded contexts must use contracts if cross-BC reads are required
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
   /// - Success(<see cref="CarDto"/>) if the car exists
   /// - NotFound if the car does not exist
   /// </summary>
   Task<Result<CarDto>> FindByIdAsync(
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
   /// - Success(<see cref="PagedResult{T}"/>)
   ///   - Items may be empty if no cars match the criteria
   /// - Invalid if paging or sorting parameters are invalid
   /// </summary>
   Task<Result<PagedResult<CarListItemDto>>> SearchAsync(
      CarSearchFilter filter,
      PageRequest page,
      SortRequest sort,
      CancellationToken ct
   );

   /// <summary>
   /// Returns availability and pricing information per car category
   /// for the given rental period (SE-1 / SE-2).
   ///
   /// Business meaning:
   /// - Used during public vehicle search:
   ///   - SE-1: show available categories + calculated price
   ///   - SE-2: show the same result filtered by categories and with
   ///          up to N example cars per category
   ///
   /// Availability rules:
   /// - Capacity is computed per category:
   ///   - capacity = number of cars in the category that are operationally usable
   ///   - blocked  = number of CONFIRMED reservations overlapping the period
   ///   - available = max(0, capacity - blocked)
   ///
   /// Pricing rules:
   /// - Price is calculated from:
   ///   - a base price per day per category
   ///   - discount tiers depending on rental length
   ///     (e.g. ≥3, ≥7, ≥14, ≥30 days)
   ///
   /// Example cars:
   /// - For UI preview only (optional)
   /// - Returned cars are concrete vehicles (CarId etc.)
   /// - Example cars are selected from currently available cars
   ///   (typically excluding vehicles blocked by ACTIVE rentals)
   ///
   /// Returns:
   /// - Success with a list of <see cref="AvailibilityWithPriceDto"/>
   ///   (one entry per category; may be empty)
   /// - Failure if input is invalid (e.g. period invalid, start in past,
   ///   negative examplesPerCategory)
   /// </summary>
   /// <param name="start">Rental period start timestamp.</param>
   /// <param name="end">Rental period end timestamp.</param>
   /// <param name="examplesPerCategory">
   /// Number of example cars to include per category.
   /// Use 0 to disable example cars.
   /// </param>
   /// <param name="ct">Cancellation token.</param>
   Task<Result<IReadOnlyList<AvailibilityWithPriceDto>>> GetAvailabilityWithPriceByCategoryAsync(
      DateTimeOffset start,
      DateTimeOffset end,
      int examplesPerCategory,
      CancellationToken ct
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist GetAvailabilityWithPriceByCategoryAsync?
 * ------------------------------------------------
 * Diese Methode ist die ReadModel-Umsetzung der Such-User-Stories:
 *
 * SE-1:
 * - "Verfügbare Fahrzeugkategorien für einen Zeitraum suchen"
 * - "Preis anzeigen" (Preis/Tag + Rabattstaffel + Gesamtpreis)
 *
 * SE-2:
 * - "Suchergebnis nach Kategorien filtern"
 * - "Bis zu N Beispiel-Fahrzeuge anzeigen" (UI-Vorschau)
 *
 *
 * Warum gehört das in ICarReadModel (und nicht in Contracts)?
 * ----------------------------------------------------------
 * - Es ist kein BC-zu-BC Contract.
 * - Es ist eine UI-/HTTP-Read-Anforderung (Search Screen).
 * - Kein anderer BC fragt Preise ab.
 *
 * Deshalb:
 * - ReadModel (UI) → ICarReadModel
 * - Pick-up (BC-intern / Rentals) → ICarReadContract
 *
 *
 * Wie wird Verfügbarkeit berechnet?
 * ---------------------------------
 * Verfügbarkeit in der Suche ist Kategorie-basiert:
 * - capacity:
 *   Anzahl Fahrzeuge einer Kategorie, die grundsätzlich vermietbar sind
 *   (z.B. Status == Available)
 *
 * - blocked:
 *   Anzahl CONFIRMED Reservierungen der Kategorie,
 *   die zeitlich überlappen (Half-open Interval [start,end))
 *
 * - available:
 *   max(0, capacity - blocked)
 *
 * Wichtig:
 * - Nur CONFIRMED Reservierungen blockieren Kapazität
 * - Draft/Cancelled/Expired blockieren NICHT
 *
 *
 * Wie werden Beispiel-Fahrzeuge gewählt?
 * -------------------------------------
 * Beispiel-Fahrzeuge sind nur "Preview"-Objekte für die UI.
 * Sie sind NICHT verbindlich reserviert.
 *
 * Typisch:
 * - Auswahl aus aktuell verfügbaren Autos
 * - Blockade über ACTIVE Rentals (CarId) beachten
 *
 *
 * Preisstaffel:
 * -------------
 * Die Preisstaffel ist bewusst simpel:
 * - Basispreis pro Tag je Kategorie
 * - Rabatte ab definierter Mietdauer (z.B. 3/7/14/30 Tage)
 *
 * Kein eigener Abrechnungs-BC:
 * - Für die Vorlesung/MVP bewusst im Cars-ReadModel integriert
 * - Fachlich später auslagerbar (ohne jetzt neue Komplexität)
 *
 * =====================================================================
 */
