using CarRentalApi.Modules.Employees.Domain.Aggregates;

namespace CarRentalApi.Modules.Employees.Domain;

/// <summary>
/// Repository interface for the Employee aggregate.
///
/// Purpose:
/// - Provides access to Employee aggregates for application use cases
/// - Encapsulates persistence concerns behind an abstraction
/// - Supports domain-specific lookup and uniqueness checks required by the EM workflows
///
/// Architectural intent:
/// - Part of the Domain Layer (Repository abstraction)
/// - Implemented in the Infrastructure Layer (e.g. EF Core)
/// - Used by Application UseCases (write side)
///
/// Important:
/// - This repository works with domain aggregates (Employee)
/// - It must NOT return DTOs/projections (that is the job of ReadModels)
/// - It should return tracked aggregates (for state changes)
/// </summary>
public interface IEmployeeRepository {

   // ------------------------------------------------------------------
   // Queries (0..1)
   // ------------------------------------------------------------------

   /// <summary>
   /// Loads an employee aggregate by its identifier.
   ///
   /// Returns:
   /// - The employee aggregate if it exists
   /// - Null if no employee with the given id exists
   /// </summary>
   Task<Employee?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   );

   /// <summary>
   /// Loads an employee aggregate by its personnel number.
   ///
   /// Business meaning:
   /// - Used to look up employees via their unique personnel number
   ///
   /// Returns:
   /// - The employee aggregate if it exists
   /// - Null if no employee with the given personnel number exists
   /// </summary>
   Task<Employee?> FindByPersonnelNumberAsync(
      string personnelNumber,
      CancellationToken ct
   );

   /// <summary>
   /// Checks whether an employee with the given personnel number exists.
   ///
   /// Business meaning:
   /// - Used to enforce uniqueness during employee creation
   ///
   /// Returns:
   /// - True if an employee with the given personnel number exists
   /// - False otherwise
   /// </summary>
   Task<bool> ExistsPersonnelNumberAsync(
      string personnelNumber,
      CancellationToken ct
   );

   /// <summary>
   /// Checks whether an employee with the given email address exists.
   ///
   /// Business meaning:
   /// - Used to enforce uniqueness during employee creation
   ///
   /// Returns:
   /// - True if an employee with the given email exists
   /// - False otherwise
   /// </summary>
   Task<bool> ExistsEmailAsync(
      string email,
      CancellationToken ct
   );

   // ------------------------------------------------------------------
   // Queries (0..n)
   // ------------------------------------------------------------------

   /// <summary>
   /// Returns all employees who are administrators.
   ///
   /// Business meaning:
   /// - Admins are employees with AdminRights != None
   ///
   /// Returns:
   /// - A list of admin employees
   /// - The list may be empty
   /// </summary>
   Task<IReadOnlyList<Employee>> SelectAdminsAsync(
      CancellationToken ct
   );

   // ------------------------------------------------------------------
   // Commands
   // ------------------------------------------------------------------

   /// <summary>
   /// Adds a new employee aggregate to the persistence context.
   ///
   /// Technical notes:
   /// - This method is synchronous by design
   /// - The aggregate is attached to the DbContext and tracked
   /// - Persistence is finalized via UnitOfWork.SaveAllChangesAsync
   /// </summary>
   void Add(Employee employee);
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist IEmployeeRepository?
 * ----------------------------
 * IEmployeeRepository ist das Repository-Interface für das
 * Employee-Aggregate im Employees-Bounded-Context.
 *
 * Es kapselt den Zugriff auf Employee-Aggregate und liefert
 * TRACKING-Entitäten (z.B. EF Core), damit UseCases Zustandsänderungen
 * am Aggregate durchführen und danach speichern können.
 *
 *
 * Warum ExistsPersonnelNumberAsync / ExistsEmailAsync?
 * ----------------------------------------------------
 * Diese beiden Methoden werden im UseCase "Employee anlegen" benötigt,
 * um fachliche Eindeutigkeitsregeln durchzusetzen:
 * - PersonnelNumber muss eindeutig sein
 * - Email muss eindeutig sein
 *
 * Alternative wäre:
 * - FindByPersonnelNumberAsync / FindByEmailAsync
 * aber Exists* ist oft effizienter (nur bool, keine Entity).
 *
 *
 * Was ist IEmployeeRepository NICHT?
 * ----------------------------------
 * - Kein ReadModel (keine DTOs, keine Projektionen, kein Paging)
 * - Kein UseCase (keine Orchestrierung)
 * - Kein Domain Service
 *
 * Für Listen/Filter/Details gilt:
 * → IEmployeeReadModel (AsNoTracking + Projektionen)
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Fachliche Regeln & Zustandsautomaten:
 *   → Employee Aggregate (Domain Layer)
 *
 * - Persistenzdetails (EF Core):
 *   → Infrastructure (EmployeeRepositoryEf)
 *
 * - Lesen/Listen/Filtern:
 *   → Application ReadModel
 *
 * =====================================================================
 */
