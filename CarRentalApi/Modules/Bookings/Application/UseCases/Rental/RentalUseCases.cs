using CarRentalApi.BuildingBlocks;
using CarRentalApi.Domain.UseCases.Rentals;
using CarRentalApi.Modules.Bookings.Application.UseCases;
using CarRentalApi.Modules.Rentals.Domain.Enums;

namespace CarRentalApi.Modules.Rentals.Application.UseCases;

/// <summary>
/// Facade for rental-related application use cases.
///
/// Purpose:
/// - Provides a single entry point for all rental write operations
/// - Delegates execution to specialized use case implementations
/// - Simplifies dependency injection for controllers
///
/// Architectural intent:
/// - Part of the Application Layer
/// - Acts as a thin facade / coordinator
/// - Does NOT contain business logic
///
/// Design notes:
/// - This class intentionally does not use async/await
/// - All calls are simple pass-through delegations
/// - Error handling and transactional behavior are handled
///   by the underlying use case implementations
/// </summary>
public sealed class RentalUseCases(
   RentalUcPickup pickupUc,
   RentalUcReturn returnUc
) : IRentalUseCases {

   /// <summary>
   /// Performs the pick-up of a confirmed reservation and creates a rental.
   ///
   /// Delegation:
   /// - Forwards the call to <see cref="RentalUcPickup"/>
   /// - No additional logic is applied at this level
   ///
   /// Returns:
   /// - The result produced by the underlying use case
   /// </summary>
   public Task<Result<Guid>> PickupAsync(
      Guid reservationId,
      RentalFuelLevel fuelOut,
      int kmOut,
      CancellationToken ct
   ) =>
      pickupUc.ExecuteAsync(reservationId, fuelOut, kmOut, ct);

   /// <summary>
   /// Performs the return of an active rental and closes it.
   ///
   /// Delegation:
   /// - Forwards the call to <see cref="RentalUcReturn"/>
   /// - No additional logic is applied at this level
   ///
   /// Returns:
   /// - The result produced by the underlying use case
   /// </summary>
   public Task<Result> ReturnAsync(
      Guid rentalId,
      RentalFuelLevel fuelIn,
      int kmIn,
      CancellationToken ct
   ) =>
      returnUc.ExecuteAsync(rentalId, fuelIn, kmIn, ct);
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Hinweise
 * =====================================================================
 *
 * Was ist RentalUseCases?
 * -----------------------
 * RentalUseCases ist eine Fassade (Facade) über mehrere
 * konkrete Anwendungsfall-Implementierungen (UseCase-Klassen).
 *
 * Sie bündelt fachlich zusammengehörige Use Cases:
 * - Pick-up (Abholung)
 * - Return (Rückgabe)
 *
 * und stellt sie über ein gemeinsames Interface (IRentalUseCases)
 * der Außenwelt (z.B. Controllern) zur Verfügung.
 *
 *
 * Warum gibt es diese Klasse überhaupt?
 * -------------------------------------
 * Ohne diese Fassade müssten Controller mehrere UseCases
 * direkt injizieren:
 *
 *   - RentalUcPickup
 *   - RentalUcReturn
 *
 * Die Fassade:
 * - reduziert die Abhängigkeiten im Controller
 * - schafft einen klaren Einstiegspunkt pro Bounded Context
 * - erleichtert Tests und spätere Erweiterungen
 *
 *
 * Warum kein async / await?
 * --------------------------
 * Diese Klasse:
 * - startet keine eigenen asynchronen Operationen
 * - fügt keine Logik hinzu
 * - delegiert ausschließlich an andere async-Methoden
 *
 * Ein async/await hier würde:
 * - keinen Mehrwert bringen
 * - zusätzlichen Overhead erzeugen
 * - nur unnötigen Boilerplate-Code hinzufügen
 *
 * Daher:
 * - bewusstes Pass-Through-Design
 *
 *
 * Abgrenzung zu anderen Klassen:
 * -------------------------------
 * - Fachliche Regeln:
 *   → Rental Aggregate (Domain Layer)
 *
 * - Konkrete UseCase-Logik:
 *   → RentalUcPickup, RentalUcReturn
 *
 * - Lesen von Daten:
 *   → IRentalReadModel
 *
 * - HTTP / API:
 *   → Controller
 *
 *
 * Typischer Aufbau im Application Layer:
 * --------------------------------------
 * Controller
 *    → IRentalUseCases (Facade)
 *        → RentalUcPickup / RentalUcReturn
 *            → Domain + Repository + UnitOfWork
 *
 * Dadurch:
 * - klare Schichtentrennung
 * - sehr gut testbare UseCases
 * - saubere DDD- und CQRS-Struktur
 *
 * =====================================================================
 */
