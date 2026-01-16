using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Cars.Application.Contracts.Dto;

namespace CarRentalApi.Modules.Cars.Application.Contracts;

/// <summary>
/// Read-only BC-to-BC contract of the Cars (Fleet Management) bounded context.
///
/// Purpose:
/// - Provides read-only access to concrete car availability
///   for the Rentals bounded context
/// - Used exclusively during pick-up to assign a specific vehicle
/// - Returns lightweight contract DTOs (projections)
///
/// Typical callers:
/// - Rentals BC during pick-up (car assignment)
///
/// Architectural notes:
/// - This is an internal BC-to-BC contract
/// - It must not expose domain aggregates or domain behavior
/// - It must not contain search or pricing logic
/// - All operations are strictly read-only
/// </summary>
public interface ICarReadContract {

   /// <summary>
   /// Finds a single available car for the given category and rental period.
   ///
   /// Business meaning:
   /// - Used during pick-up (RE-1 / RE-2)
   /// - Assigns exactly one concrete vehicle to a rental
   ///
   /// Availability rules:
   /// - car category must match
   /// - car must be operationally usable
   /// - car must not be blocked by an ACTIVE rental
   ///   with an overlapping CONFIRMED reservation
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
   /// for the given category and rental period.
   ///
   /// Business meaning:
   /// - Used during pick-up as a fallback list
   /// - Allows an employee to manually choose
   ///   from multiple available vehicles
   ///
   /// Availability rules are identical to
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
 * ICarReadContract ist ein BC-zu-BC Read-Contract
 * des Cars (Fleet Management) Bounded Contexts
 * für den Rentals Bounded Context.
 *
 * Er wird ausschließlich beim Pick-up verwendet,
 * um ein konkretes Fahrzeug (CarId) zuzuweisen.
 *
 *
 * Welche User Stories werden unterstützt?
 * ---------------------------------------
 * RE-1:
 * - "Mietvorgang aus bestätigter Reservierung starten (Pick-up)"
 *
 * RE-2:
 * - "Beim Pick-up verfügbare Fahrzeuge anzeigen
 *    und ein konkretes Fahrzeug zuweisen"
 *
 *
 * Welche User Stories werden NICHT unterstützt?
 * ---------------------------------------------
 * - SE-1 / SE-2 (Suche, Verfügbarkeit, Preise)
 *   → laufen ausschließlich über ICarReadModel
 *
 * - Preisberechnung
 * - Kategorie-basierte Suche
 *
 *
 * Warum kein Pricing und keine Suche hier?
 * ----------------------------------------
 * - Pick-up ist ein operativer Vorgang
 * - Es wird genau ein Fahrzeug benötigt
 * - Preise wurden bereits vorher ermittelt
 *
 * Deshalb:
 * - Suche + Preis → Cars.ReadModel (UI)
 * - Pick-up → Cars.ReadContract (BC-zu-BC)
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
 * - Es werden keine Fahrzeuge reserviert
 * - Es werden keine Preise berechnet
 * - Es werden keine Zustände geändert
 *
 * =====================================================================
 */
