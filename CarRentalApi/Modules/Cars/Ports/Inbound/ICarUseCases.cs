using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Cars.Domain.Aggregates;

namespace CarRentalApi.Modules.Cars.Ports.Inbound;

/// <summary>
/// Application use cases for the Cars bounded context (Fleet Management).
///
/// Purpose:
/// - Defines all state-changing operations related to cars
/// - Serves as the inbound application boundary (port)
/// - Is called by API controllers or other application services
///
/// Scope:
/// - Create new cars
/// - Change operational state (maintenance, active, retired)
///
/// Architectural intent:
/// - Part of the Application Layer (Inbound Port)
/// - Exposes COMMAND use cases (write side)
/// - Hides concrete implementations behind an interface
///
/// Important:
/// - This interface represents write operations only
/// - It must NOT be used for read/query scenarios
/// - It must NOT be implemented in the Domain Layer
///
/// Result policy:
/// - Success:
///   - Returns created aggregates or completes without payload
/// - Failure:
///   - Returns domain-specific errors (validation, conflicts, not found)
/// </summary>
public interface ICarUseCases {

   /// <summary>
   /// Creates a new car and adds it to the fleet.
   ///
   /// Business meaning:
   /// - Registers a physical vehicle in the system
   /// - Makes the car available for future reservations and rentals
   ///
   /// Preconditions:
   /// - The provided data must be valid
   /// - The license plate must be unique
   ///
   /// Side effects:
   /// - Creates a new Car aggregate
   /// - Persists it in the database
   ///
   /// Returns:
   /// - Success(<see cref="Car"/>) containing the created car
   /// - Invalid if input data is invalid
   /// - Conflict if the license plate or id already exists
   /// </summary>
   Task<Result<Car>> CreateAsync(
      CarCategory category,
      string manufacturer,
      string model,
      string licensePlate,
      string? id,
      CancellationToken ct
   );

   /// <summary>
   /// Sends a car to maintenance.
   ///
   /// Business meaning:
   /// - Marks the car as temporarily unavailable
   /// - Prevents the car from being assigned to new rentals
   ///
   /// Preconditions:
   /// - The car must exist
   /// - The car must not already be retired
   ///
   /// Side effects:
   /// - Changes the operational status of the car
   ///
   /// Returns:
   /// - Success if the car was successfully sent to maintenance
   /// - NotFound if the car does not exist
   /// - Conflict if the car cannot enter maintenance in its current state
   /// </summary>
   Task<Result> SendToMaintainanceAsync(
      Guid carId,
      CancellationToken ct
   );

   /// <summary>
   /// Returns a car from maintenance back into active service.
   ///
   /// Business meaning:
   /// - Marks the car as available again
   /// - Allows the car to be used for future rentals
   ///
   /// Preconditions:
   /// - The car must exist
   /// - The car must currently be in maintenance
   ///
   /// Side effects:
   /// - Changes the operational status of the car
   ///
   /// Returns:
   /// - Success if the car was successfully returned from maintenance
   /// - NotFound if the car does not exist
   /// - Conflict if the car is not in maintenance
   /// </summary>
   Task<Result> ReturnFromMaintainanceAsync(
      Guid carId,
      CancellationToken ct
   );

   /// <summary>
   /// Retires a car permanently from the fleet.
   ///
   /// Business meaning:
   /// - Removes the car from active operation
   /// - The car can no longer be rented or maintained
   ///
   /// Preconditions:
   /// - The car must exist
   /// - The car must not already be retired
   ///
   /// Side effects:
   /// - Marks the car as retired
   ///
   /// Returns:
   /// - Success if the car was successfully retired
   /// - NotFound if the car does not exist
   /// - Conflict if the car cannot be retired in its current state
   /// </summary>
   Task<Result> RetireAsync(
      Guid carId,
      CancellationToken ct
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist ICarUseCases?
 * ---------------------
 * ICarUseCases ist der Inbound Port (Eingangsschnittstelle)
 * für den Cars-/Fleet-Bounded-Context.
 *
 * Er definiert alle fachlichen Anwendungsfälle, die den Zustand
 * von Fahrzeugen verändern.
 *
 * Es handelt sich um die WRITE-Seite des Systems.
 *
 *
 * Was ist ICarUseCases NICHT?
 * ---------------------------
 * - Kein ReadModel (keine Listen, keine Suchen)
 * - Kein Repository (keine Persistenzdetails)
 * - Kein Domain Service
 * - Kein Aggregate
 *
 *
 * Warum ein Inbound Port?
 * -----------------------
 * - Controller hängen nur von Interfaces ab
 * - Implementierungen können ausgetauscht werden
 * - Klare Trennung zwischen API und Anwendungslogik
 *
 *
 * Fachliche Einordnung der Use Cases:
 * ----------------------------------
 * - Create:
 *   - Fahrzeug wird erstmals im System registriert
 *
 * - SendToMaintenance:
 *   - Fahrzeug ist temporär nicht verfügbar
 *
 * - ReturnFromMaintenance:
 *   - Fahrzeug wird wieder einsatzbereit
 *
 * - Retire:
 *   - Fahrzeug wird dauerhaft außer Betrieb genommen
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Fachliche Regeln & Zustandsautomaten:
 *   → Car Aggregate (Domain Layer)
 *
 * - Persistenz:
 *   → CarRepository (Infrastructure)
 *
 * - Lesen / Anzeigen:
 *   → ICarReadModel oder Read APIs
 *
 *
 * Typische Aufrufer:
 * ------------------
 * - API Controller
 * - Application Services
 *
 * =====================================================================
 */
