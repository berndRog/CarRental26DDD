using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Cars.Application.Contracts.Dto;

namespace CarRentalApi.Modules.Cars.Application.Contracts;

/// <summary>
/// Read-only facade of the Cars (Fleet Management) bounded context
/// for other bounded contexts.
///
/// Purpose:
/// - Provides read-only access to car availability information
///   without exposing the car domain model
/// - Returns lightweight contract DTOs (projections)
///
/// Typical callers:
/// - Bookings BC during search (availability by category)
/// - Rentals BC during pick-up (car assignment)
///
/// Architectural notes:
/// - This is an internal, BC-to-BC contract
/// - It must not expose aggregates or domain behavior
/// - All operations are read-only
/// </summary>
public interface ICarReadContract {

   /// <summary>
   /// Returns availability information per car category
   /// for the given rental period.
   ///
   /// Business meaning:
   /// - Used during vehicle search (SE-1 / SE-2)
   /// - Provides an overview per category, not per individual car
   ///
   /// For each category, the result contains:
   /// - the number of available cars
   /// - a limited list of example cars (e.g. for UI preview)
   ///
   /// Availability must consider:
   /// - car category
   /// - operational status (e.g. Available / InMaintenance / Retired)
   /// - overlapping rentals and confirmed reservations
   ///
   /// Returns:
   /// - Success with a list of <see cref="CarAvailabilityContractDto"/>
   ///   (may be empty if no categories are available)
   /// - Failure if input parameters are invalid or availability
   ///   cannot be evaluated
   /// </summary>
   Task<Result<IReadOnlyList<CarAvailabilityContractDto>>> GetAvailabilityByCategoryAsync(
      DateTimeOffset start,
      DateTimeOffset end,
      IReadOnlyList<CarCategory>? categories,
      int examplesPerCategory,
      CancellationToken ct
   );

   /// <summary>
   /// Finds a single available car for the given category and period.
   ///
   /// Business meaning:
   /// - Used during pick-up to assign exactly one car to a rental
   ///
   /// Availability must consider:
   /// - car category
   /// - operational status (e.g. Available / InMaintenance / Retired)
   /// - overlapping rentals for the given period
   ///
   /// Returns:
   /// - Success with a suitable <see cref="CarContractDto"/> if available
   /// - Success with null if no car can be assigned
   /// - Failure if input is invalid or availability cannot be evaluated
   /// </summary>
   Task<Result<CarContractDto?>> FindAvailableCarAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      CancellationToken ct
   );

   /// <summary>
   /// Selects up to <paramref name="limit"/> alternative available cars
   /// for the given category and period.
   ///
   /// Business meaning:
   /// - Used during pick-up as a fallback list when the first choice
   ///   cannot be used
   /// - Supports manual selection (e.g. employee chooses one of
   ///   the available candidates)
   ///
   /// Availability rules are the same as for
   /// <see cref="FindAvailableCarAsync"/>.
   ///
   /// Returns:
   /// - Success with a list of available car candidates
   ///   (may be empty)
   /// - Failure if input is invalid or the query cannot be executed
   /// </summary>
   Task<Result<IReadOnlyList<CarContractDto>>> SelectAvailableCarsAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      int limit,
      CancellationToken ct
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Hinweise
 * =====================================================================
 *
 * Was ist ICarReadContract?
 * ------------------------
 * ICarReadContract ist die zentrale Read-Schnittstelle des
 * Cars (Fleet Management) Bounded Contexts für andere BCs.
 *
 * Sie stellt ausschließlich lesenden Zugriff auf
 * Fahrzeugverfügbarkeiten bereit und gibt nur
 * Contract-DTOs (Projektionen) zurück.
 *
 *
 * Welche Use Cases werden unterstützt?
 * ------------------------------------
 * 1) Fahrzeugsuche (SE-1 / SE-2):
 *    - Abfrage der Verfügbarkeit pro Fahrzeugkategorie
 *    - Anzeige der Anzahl verfügbarer Fahrzeuge
 *    - Anzeige von Beispiel-Fahrzeugen (z.B. max. 5)
 *
 *    → Methode: GetAvailabilityByCategoryAsync
 *
 * 2) Pick-up / Fahrzeugzuweisung (Rentals):
 *    - Ermittlung eines konkreten verfügbaren Fahrzeugs
 *    - Optional: Auswahl aus mehreren Kandidaten
 *
 *    → Methoden:
 *      - FindAvailableCarAsync
 *      - SelectAvailableCarsAsync
 *
 *
 * Was ist ICarReadContract NICHT?
 * ------------------------------
 * - Kein Repository
 * - Kein Domain Service
 * - Kein Use Case
 * - Keine Änderungslogik
 *
 * Insbesondere:
 * - Es werden keine Zustände geändert
 * - Es werden keine Fahrzeuge reserviert oder zugewiesen
 * - Es werden keine Preise berechnet
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Preisberechnung:
 *   → Bookings.Domain (PriceCalculationPolicy)
 *
 * - Kapazitätsprüfung bei Reservierungen:
 *   → Bookings.Domain.Policies (ReservationConflictPolicy)
 *
 * - Zustandsänderungen (Pick-up / Return):
 *   → Rentals Use Cases
 *
 *
 * Architekturentscheidung:
 * ------------------------
 * - Eine gemeinsame Read-Schnittstelle wird bewusst beibehalten,
 *   um eine Zersplitterung in viele kleine Contracts zu vermeiden.
 * - Die fachliche Trennung erfolgt über Methoden,
 *   nicht über zusätzliche Interfaces.
 *
 * =====================================================================
 */
