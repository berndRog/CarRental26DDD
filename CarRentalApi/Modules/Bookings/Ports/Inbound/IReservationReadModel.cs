using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Dto;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
namespace CarRentalApi.Modules.Bookings.Application.ReadModel;

/// <summary>
/// Read-only query model for the Booking bounded context (Reservations).
///
/// Purpose:
/// - Used by API controllers and UI-facing endpoints
/// - Provides read access to reservation data via projections (DTOs)
/// - Supports detail views and list/search screens
/// - Optimized for query use-cases (no tracking, no aggregates)
///
/// Architectural intent:
/// - Serves HTTP/API read endpoints only
/// - Separates read concerns from write/use-case logic
/// - Allows efficient database projections and indexing
///
/// Important:
/// - This is NOT a bounded-context facade
/// - Other bounded contexts must use IBookingsReadApi (Contracts), if exposed
/// - This interface must NOT be used by domain services or use cases
///   to enforce invariants or modify state
///
/// Result policy:
/// - Success:
///   - Single-item queries return the requested projection
///   - Search queries return paged result sets (may be empty)
/// - NotFound:
///   - Returned for single-item queries when the reservation does not exist
/// - Invalid:
///   - Returned when input parameters are invalid
///     (e.g. paging/sorting values out of range, invalid filter combinations)
/// </summary>
public interface IReservationReadModel {

   /// <summary>
   /// Returns detailed information about a single reservation.
   ///
   /// Business meaning:
   /// - Used by reservation detail views (customer or back-office)
   /// - Shows the current state of the reservation lifecycle
   ///   (e.g. Draft, Confirmed, Cancelled, Expired)
   ///
   /// Technical notes:
   /// - Read-only projection
   /// - No domain logic or state transitions are executed
   /// - Data is retrieved using no-tracking queries
   ///
   /// Returns:
   /// - Success(<see cref="ReservationDetailsDto"/>) if the reservation exists
   /// - NotFound if no reservation with the given id exists
   /// </summary>
   Task<Result<ReservationDetailsDto>> FindByIdAsync(
      Guid reservationId,
      CancellationToken ct = default
   );

   /// <summary>
   /// Searches reservations using flexible filter, paging and sorting parameters.
   ///
   /// Business meaning:
   /// - Used by reservation list and search screens
   /// - Supports common scenarios:
   ///   - "My reservations" (filter by customer)
   ///   - Back-office administration
   ///   - Filtering by status or rental period
   ///
   /// Technical notes:
   /// - Paging is controlled via <see cref="PageRequest"/>
   /// - Sorting is controlled via <see cref="SortRequest"/>
   /// - Queries are optimized for read access (no aggregates, no tracking)
   /// - Sorting must be restricted to a predefined set of allowed fields
   ///
   /// Returns:
   /// - Success(<see cref="PagedResult{ReservationListItemDto}"/>)
   ///   - Items may be empty if no reservations match the criteria
   /// - Invalid if paging or sorting parameters are invalid
   /// </summary>
   Task<Result<PagedResult<ReservationListItemDto>>> SearchAsync(
      ReservationSearchFilter filter,
      PageRequest page,
      SortRequest sort,
      CancellationToken ct = default
   );
}

/* =====================================================================
   * Deutsche Architektur- und Didaktik-Hinweise
   * =====================================================================
   *
   * Was ist IRentalUseCases?
   * ------------------------
   * IRentalUseCases ist die öffentliche Anwendungsfall-Schnittstelle
   * (Application Layer) für den Rentals-Bounded-Context.
   *
   * Sie definiert die fachlichen Kommandos (Commands), die den Zustand
   * des Systems verändern:
   * - Pickup (Abholung) erzeugt bzw. startet eine Miete
   * - Return (Rückgabe) beendet eine Miete
   *
   * IRentalUseCases kapselt die Orchestrierung:
   * - Laden/Speichern von Aggregaten über Repositories
   * - Aufruf von Domain-Methoden (Invarianten, Zustandsübergänge)
   * - Transaktionale Konsistenz (UnitOfWork)
   * - Zeit/Clock, Logging, ggf. Policies
   *
   *
   * Was ist IRentalUseCases NICHT?
   * ------------------------------
   * - Kein ReadModel (keine Queries, keine Such-/Listen-Endpunkte)
   * - Kein Repository (keine Persistenz-Details, kein EF Core im Interface)
   * - Kein Domain Service (keine fachlichen Regeln im Application Layer)
   * - Kein Aggregate (keine Zustandsdatenhaltung, keine Entity)
   *
   * Insbesondere:
   * - Der UseCase enthält keine fachlichen Regeln „im Code verteilt“,
   *   sondern delegiert Regeln an das Domain Model (Rental Aggregate).
   *
   *
   * Warum gibt es zwei Methoden (PickupAsync / ReturnAsync)?
   * --------------------------------------------------------
   * Fachlicher Hintergrund:
   * - Ein Rental entsteht erst bei der Abholung (Pick-up).
   *   Vorher existiert „nur“ eine bestätigte Reservation.
   * - Ein Rental wird bei der Rückgabe geschlossen (Return).
   *
   * Die beiden UseCases modellieren genau diese fachlichen Zustandsübergänge:
   * - Reservation (Confirmed) → Rental (Active)   via PickupAsync
   * - Rental (Active)        → Rental (Returned) via ReturnAsync
   *
   *
   * Abgrenzung zu anderen Schichten:
   * --------------------------------
   * - Lesen/Anzeigen von Mietdaten:
   *   → IRentalReadModel (AsNoTracking + Projektionen)
   *
   * - Fachliche Regeln und Invarianten:
   *   → Rental Aggregate (Domain Layer)
   *     z.B.:
   *     - FuelLevel 0..100
   *     - KmIn >= KmOut
   *     - Statusübergänge nur in erlaubter Reihenfolge
   *
   * - Persistenz / EF Core:
   *   → RentalRepository (Infrastructure)
   *
   * - Cross-BC Datenzugriffe (z.B. Reservation confirmed?):
   *   → Contracts / ReadApi des anderen BC (z.B. IReservationsReadApi)
   *
   *
   * Typisches Implementierungsdetail:
   * ---------------------------------
   * - PickupAsync:
   *   - prüft Reservation existiert und ist Confirmed (über ReadApi/Repository)
   *   - verhindert Doppel-Pickup (Conflict)
   *   - erzeugt Rental Aggregate + setzt Status Active + speichert
   *
   * - ReturnAsync:
   *   - lädt Rental Aggregate (Tracking, weil Zustandsänderung)
   *   - führt Domain-Übergang Active → Returned aus
   *   - speichert + commit über UnitOfWork
   *
   * Dadurch:
   * - klare CQRS-Trennung (ReadModel vs UseCases)
   * - zentrale fachliche Anwendungsfälle
   * - saubere BC-Grenzen
   * - gut testbar (Unit Tests auf UseCases)
   *
   * =====================================================================
*/