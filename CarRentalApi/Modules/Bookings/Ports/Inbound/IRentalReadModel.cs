using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Rentals.Application.ReadModel.Dto;

namespace CarRentalApi.Modules.Rentals.Application.ReadModel;

/// <summary>
/// Read-only query model for the Rentals bounded context.
///
/// Purpose:
/// - Used by API controllers and UI-facing endpoints
/// - Provides read access to rental data via projections (DTOs)
/// - Supports detail views and cross-context lookup scenarios
/// - Optimized for query use-cases (no tracking, no aggregates)
///
/// Architectural intent:
/// - Serves HTTP/API read endpoints only
/// - Separates read concerns from write/use-case logic (CQRS-style)
/// - Allows efficient database projections and indexing
///
/// Important:
/// - This is NOT a bounded-context facade
/// - Other bounded contexts must use IRentalsReadApi (Contracts), if exposed
/// - This interface must NOT be used by domain services or use cases
///   to enforce invariants or modify state
///
/// Result policy:
/// - Success:
///   - Single-item queries return the requested projection
/// - NotFound:
///   - Returned when the requested rental does not exist
/// - Invalid:
///   - Returned when input parameters are invalid
/// </summary>
public interface IRentalReadModel {

   /// <summary>
   /// Returns detailed information about a single rental.
   ///
   /// Business meaning:
   /// - Used by rental detail views (back-office, customer support)
   /// - Shows the complete rental lifecycle information
   ///   (e.g. pick-up data, return data, fuel levels, mileage)
   ///
   /// Technical notes:
   /// - Read-only projection
   /// - No domain logic or state transitions are executed
   /// - Data is retrieved using no-tracking queries
   ///
   /// Returns:
   /// - Success(<see cref="RentalDetailsDto"/>) if the rental exists
   /// - NotFound if no rental with the given id exists
   /// </summary>
   Task<Result<RentalDetailsDto>> FindByIdAsync(
      Guid rentalId,
      CancellationToken ct
   );

   /// <summary>
   /// Returns the rental id for a given reservation, if the reservation
   /// has already been picked up.
   ///
   /// Business meaning:
   /// - Used during navigation flows:
   ///   - From reservation details to rental details
   ///   - To check whether a confirmed reservation has already
   ///     transitioned into an active rental
   ///
   /// Technical notes:
   /// - Read-only lookup
   /// - No tracking, no aggregates
   /// - Returns <c>null</c> if the reservation has not been picked up yet
   ///
   /// Returns:
   /// - Success(<see cref="Guid"/>?) 
   ///   - rentalId if a rental exists for the reservation
   ///   - null if no rental exists yet
   /// - NotFound if the reservation itself does not exist
   /// </summary>
   Task<Result<Guid?>> FindRentalIdByReservationIdAsync(
      Guid reservationId,
      CancellationToken ct
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Hinweise
 * =====================================================================
 *
 * Was ist IRentalReadModel?
 * -------------------------
 * IRentalReadModel ist ein reines Read Model (Query Model) für den
 * Rentals-Bounded-Context.
 *
 * Es dient ausschließlich dazu, Mietdaten (Rentals) für:
 * - API-Endpunkte
 * - UI-Ansichten (Detailansichten, Navigation)
 * bereitzustellen.
 *
 * Es liefert KEINE Domain-Objekte, sondern nur Projektionen (DTOs).
 *
 *
 * Was ist IRentalReadModel NICHT?
 * -------------------------------
 * - Kein Domain Service
 * - Kein Use Case
 * - Kein Repository
 * - Kein Aggregate-Zugriff
 * - Keine fachlichen Invarianten
 *
 * Insbesondere:
 * - Es ändert keinen Zustand
 * - Es führt keine Statusübergänge aus (Pick-up / Return)
 * - Es kennt keine Geschäftsregeln
 *
 *
 * Warum gibt es eine Methode
 * FindRentalIdByReservationIdAsync?
 * ---------------------------------
 * Fachlicher Hintergrund:
 * - Eine Reservation kann bestätigt sein, aber noch nicht abgeholt
 * - Erst beim Pick-up entsteht ein Rental
 *
 * Diese Methode erlaubt:
 * - festzustellen, ob aus einer Reservation bereits ein Rental wurde
 * - Navigation von "Reservation → Rental", ohne Domain-Logik auszuführen
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Statuswechsel (Pick-up / Return):
 *   → IRentalUseCases
 *
 * - Fachliche Regeln (z.B. FuelLevel, Kilometer, Status):
 *   → Rental Aggregate (Domain Layer)
 *
 * - Persistenz:
 *   → Repository (Infrastructure)
 *
 *
 * Typisches Implementierungsdetail:
 * ---------------------------------
 * - EF Core mit AsNoTracking()
 * - Projektionen via Select(...)
 * - Keine Navigation Properties
 *
 * Dadurch:
 * - hohe Performance
 * - keine Seiteneffekte
 * - saubere CQRS-Trennung
 *
 * =====================================================================
 */