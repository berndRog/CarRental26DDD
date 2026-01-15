using CarRentalApi.BuildingBlocks;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
using CarRentalApi.Modules.Employees.Domain.Enums;
namespace CarRentalApi.Modules.Employees.Ports.Inbound;

/// <summary>
/// Application use cases for the Employees bounded context (write side).
///
/// Purpose:
/// - Defines all state-changing operations related to employees
/// - Serves as the inbound application boundary (port)
/// - Is used by API controllers and application services
///
/// Scope:
/// - Employee creation
/// - Employee deactivation
/// - Setting or changing administrative rights
///
/// Architectural intent:
/// - Part of the Application Layer
/// - Represents COMMAND use cases (CQRS write side)
/// - Hides concrete implementations behind an interface
///
/// Important:
/// - This interface exposes write operations only
/// - It must NOT be used for read/query scenarios
/// - It must NOT return projections or DTOs
///
/// Result policy:
/// - Success:
///   - Returns identifiers or completes without payload
/// - Failure:
///   - Returns domain-specific errors (validation, conflict, not found)
/// </summary>
public interface IEmployeeUseCases {

   /// <summary>
   /// Creates a new employee.
   ///
   /// Business meaning:
   /// - Registers a new employee in the system
   /// - Assigns an initial set of administrative rights
   ///
   /// Preconditions:
   /// - Personnel number must be unique
   /// - Email address must be valid
   ///
   /// Side effects:
   /// - Creates a new Employee aggregate
   /// - Persists it in the database
   ///
   /// Returns:
   /// - Success(Guid) containing the new employee id
   /// - Invalid if input data is invalid
   /// - Conflict if personnel number or email already exists
   /// </summary>
   Task<Result<Guid>> CreateAsync(
      string firstName,
      string lastName,
      string emailString,
      string phoneString,
      string personnelNumber,
      AdminRights adminRights,
      DateTimeOffset createdAt,
      string? id = null,
      Address? address = null,
      CancellationToken ct = default
   );

   /// <summary>
   /// Deactivates an existing employee.
   ///
   /// Business meaning:
   /// - Marks an employee as inactive
   /// - Prevents the employee from performing administrative actions
   ///
   /// Preconditions:
   /// - The employee must exist
   ///
   /// Side effects:
   /// - Changes the employee status to inactive
   /// - Sets the deactivation timestamp
   ///
   /// Returns:
   /// - Success if the employee was deactivated
   /// - NotFound if the employee does not exist
   /// - Conflict if the employee is already inactive
   /// </summary>
   Task<Result> DeactivateAsync(
      Guid employeeId,
      DateTimeOffset deactivatedAt,
      CancellationToken ct = default
   );

   /// <summary>
   /// Sets or updates the administrative rights of an employee.
   ///
   /// Business meaning:
   /// - Grants or revokes permissions for administrative actions
   /// - Rights are represented as a bitmask (flags)
   ///
   /// Preconditions:
   /// - The employee must exist
   ///
   /// Side effects:
   /// - Updates the AdminRights of the employee
   ///
   /// Returns:
   /// - Success if the rights were updated
   /// - NotFound if the employee does not exist
   /// - Invalid if the provided rights value is invalid
   /// </summary>
   Task<Result> SetAdminRightsAsync(
      Guid employeeId,
      AdminRights adminRights,
      CancellationToken ct = default
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist IEmployeeUseCases?
 * --------------------------
 * IEmployeeUseCases ist der Inbound Port (Eingangsschnittstelle)
 * für alle zustandsverändernden Anwendungsfälle im
 * Employees-Bounded-Context.
 *
 * Es handelt sich um die WRITE-Seite des Systems (CQRS).
 *
 *
 * Welche User Stories werden hier abgebildet?
 * -------------------------------------------
 * - EM-1: Employee anlegen
 *   → CreateAsync
 *
 * - EM-2: Employee deaktivieren
 *   → DeactivateAsync
 *
 * - ST-3: AdminRights setzen / ändern
 *   → SetAdminRightsAsync
 *
 *
 * Was gehört bewusst NICHT hierher?
 * ---------------------------------
 * - Anzeigen von Employee-Details (ST-4)
 * - Anzeigen von Listen (ST-5)
 * - Filtern / Suchen (ST-6)
 *
 * Diese Fälle gehören in das ReadModel:
 * → IEmployeeReadModel
 *
 *
 * Warum eine UseCase-Schnittstelle?
 * ---------------------------------
 * - Controller hängen nur von Interfaces ab
 * - Klare Trennung zwischen API und Anwendungslogik
 * - Einfache Austauschbarkeit der Implementierung
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Fachliche Regeln & Invarianten:
 *   → Employee Aggregate (Domain Layer)
 *
 * - Persistenz:
 *   → EmployeeRepository (Infrastructure)
 *
 * - Lesen / UI-Abfragen:
 *   → EmployeeReadModel
 *
 *
 * Merksatz:
 * ---------
 * UseCases verändern Zustand.
 * ReadModels liefern Projektionen.
 *
 * =====================================================================
 */
