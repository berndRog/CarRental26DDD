using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;

namespace CarRentalApi.Modules.Bookings.Application.UseCases;

/// <summary>
/// Facade for reservation-related application use cases.
///
/// Purpose:
/// - Provides a single entry point for all reservation write operations
/// - Delegates execution to specialized reservation use case implementations
/// - Simplifies dependency injection for API controllers
///
/// Architectural intent:
/// - Part of the Application Layer
/// - Acts as a thin facade / coordinator
/// - Does NOT contain business logic
///
/// Design notes:
/// - This class intentionally does not use async/await
/// - All methods are simple pass-through delegations
/// - Transaction handling, validation and error mapping
///   are handled by the underlying use case implementations
/// </summary>
public sealed class ReservationUseCases(
   ReservationUcCreate createUc,
   ReservationUcChangePeriod changePeriodUc,
   ReservationUcConfirm confirmUc,
   ReservationUcCancel cancelUc,
   ReservationUcExpire expireUc
) : IReservationUseCases {

   /// <summary>
   /// Creates a new reservation in Draft state.
   ///
   /// Delegation:
   /// - Forwards the call to <see cref="ReservationUcCreate"/>
   /// - No additional logic is applied at this level
   /// </summary>
   public Task<Result<Guid>> CreateAsync(
      Guid customerId,
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      string? id,
      CancellationToken ct
   ) =>
      createUc.ExecuteAsync(customerId, category, start, end, id, ct);

   /// <summary>
   /// Changes the rental period of an existing reservation.
   ///
   /// Delegation:
   /// - Forwards the call to <see cref="ReservationUcChangePeriod"/>
   /// - No additional logic is applied at this level
   /// </summary>
   public Task<Result> ChangePeriodAsync(
      Guid reservationId,
      DateTimeOffset start,
      DateTimeOffset end,
      CancellationToken ct
   ) =>
      changePeriodUc.ExecuteAsync(reservationId, start, end, ct);

   /// <summary>
   /// Confirms a reservation.
   ///
   /// Delegation:
   /// - Forwards the call to <see cref="ReservationUcConfirm"/>
   /// - No additional logic is applied at this level
   /// </summary>
   public Task<Result> ConfirmAsync(
      Guid reservationId,
      CancellationToken ct
   ) =>
      confirmUc.ExecuteAsync(reservationId, ct);

   /// <summary>
   /// Cancels an existing reservation.
   ///
   /// Delegation:
   /// - Forwards the call to <see cref="ReservationUcCancel"/>
   /// - No additional logic is applied at this level
   /// </summary>
   public Task<Result> CancelAsync(
      Guid reservationId,
      CancellationToken ct
   ) =>
      cancelUc.ExecuteAsync(reservationId, ct);

   /// <summary>
   /// Expires outdated reservations automatically.
   ///
   /// Delegation:
   /// - Forwards the call to <see cref="ReservationUcExpire"/>
   /// - No additional logic is applied at this level
   /// </summary>
   public Task<Result<int>> ExpireAsync(
      CancellationToken ct
   ) =>
      expireUc.ExecuteAsync(ct);
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Hinweise
 * =====================================================================
 *
 * Was ist ReservationUseCases?
 * ----------------------------
 * ReservationUseCases ist eine Fassade (Facade) über mehrere konkrete
 * UseCase-Implementierungen im Booking-/Reservations-Bounded-Context.
 *
 * Sie bündelt alle zustandsverändernden Anwendungsfälle:
 * - Create
 * - ChangePeriod
 * - Confirm
 * - Cancel
 * - Expire
 *
 * und stellt sie über ein einziges Interface (IReservationUseCases)
 * nach außen bereit.
 *
 *
 * Warum gibt es diese Klasse?
 * ---------------------------
 * Ohne diese Fassade müssten Controller mehrere UseCases
 * direkt injizieren, z.B.:
 * - ReservationUcCreate
 * - ReservationUcConfirm
 * - ReservationUcCancel
 *
 * Die Fassade:
 * - reduziert die Anzahl der Abhängigkeiten im Controller
 * - schafft einen klaren Einstiegspunkt pro Bounded Context
 * - erleichtert Austausch und Tests
 *
 *
 * Warum kein async / await?
 * -------------------------
 * Diese Klasse:
 * - führt keine eigene asynchrone Logik aus
 * - fügt keinen fachlichen Mehrwert hinzu
 * - delegiert ausschließlich an andere async-Methoden
 *
 * Ein async/await würde hier:
 * - nur Boilerplate-Code erzeugen
 * - keinen zusätzlichen Nutzen bringen
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Lesen von Reservationsdaten:
 *   → IReservationReadModel
 *
 * - Fachliche Regeln und Invarianten:
 *   → Reservation Aggregate (Domain Layer)
 *
 * - Konkrete UseCase-Logik:
 *   → ReservationUcCreate, ReservationUcConfirm, ...
 *
 * - Persistenz:
 *   → ReservationRepository (Infrastructure)
 *
 *
 * Typischer Aufbau im Application Layer:
 * --------------------------------------
 * Controller
 *    → IReservationUseCases (Facade)
 *        → konkrete ReservationUc*
 *            → Domain + Repository + UnitOfWork
 *
 * Dadurch:
 * - klare Schichtentrennung
 * - gute Testbarkeit
 * - saubere DDD- & CQRS-Struktur
 *
 * =====================================================================
 */
