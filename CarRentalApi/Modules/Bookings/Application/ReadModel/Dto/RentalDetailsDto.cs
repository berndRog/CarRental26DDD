using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Bookings.Domain.Enums;

namespace CarRentalApi.Modules.Rentals.Application.ReadModel.Dto;

/// <summary>
/// Read-only projection representing detailed information about a rental.
///
/// Purpose:
/// - Used by rental detail views (API / UI)
/// - Represents the full rental lifecycle (pick-up → return)
/// - Serves as an immutable projection of the Rental aggregate
///
/// Characteristics:
/// - Immutable record with primary constructor
/// - No domain logic
/// - No navigation properties
/// - Optimized for read scenarios
/// </summary>
public sealed record RentalDetailsDto(
   Guid RentalId,

   // Foreign keys / references
   Guid ReservationId,
   Guid CarId,
   Guid CustomerId,

   // Lifecycle
   RentalStatus Status,

   // Pick-up data
   DateTimeOffset PickupAt,
   int FuelLevelOut,   // 0..100
   int KmOut,          // >= 0

   // Return data (nullable while active)
   DateTimeOffset? ReturnAt,
   int? FuelLevelIn,   // 0..100
   int? KmIn           // >= KmOut
);

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist RentalDetailsDto?
 * -------------------------
 * RentalDetailsDto ist ein ReadModel-DTO, das eine vollständige
 * Sicht auf eine Miete (Rental) bietet.
 *
 * Es bildet den gesamten Lebenszyklus ab:
 * - Abholung (Pick-up)
 * - Rückgabe (Return)
 *
 * Das DTO ist eine Projektion des Rental-Aggregates und wird
 * ausschließlich für Lesezwecke verwendet.
 *
 *
 * Warum ein immutable record mit Primärkonstruktor?
 * --------------------------------------------------
 * - garantiert vollständige Initialisierung
 * - verhindert nachträgliche Mutation
 * - ideal für EF-Core-Projektionen (Select new ...)
 * - sehr gut geeignet für CQRS Read Models
 *
 * Warum sind Rückgabe-Felder nullable?
 * -----------------------------------
 * - Ein Rental existiert bereits nach dem Pick-up
 * - Die Rückgabe erfolgt zeitlich später
 * - Während das Rental aktiv ist:
 *   - ReturnAt == null
 *   - FuelLevelIn == null
 *   - KmIn == null
 *
 * Was macht dieses DTO bewusst NICHT?
 * -----------------------------------
 * - keine Statuslogik
 * - keine Validierung (0..100, Km >= KmOut)
 * - keine Berechnungen
 * - keine Navigation zu Car / Customer / Reservation
 *
 * Diese Regeln gehören:
 * - in das Rental-Aggregate (Domain Layer)
 * - oder in UseCases (Application Layer)
 *
 * Typische Verwendung:
 * --------------------
 * - GET /api/rentals/{id}
 * - Backoffice-Ansichten
 * - Support- und Verwaltungsoberflächen
 *
 * Implementierungs-Hinweis:
 * -------------------------
 * - EF Core: AsNoTracking()
 * - Projektion über Select(...)
 * - Keine Include(...)
 *
 * =====================================================================
 */

