using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Employees.Domain;
using CarRentalApi.Modules.Employees.Domain.Enums;
using CarRentalApi.Modules.Employees.Domain.Errors;
using CarRentalApi.Modules.Employees.Infrastructure;
namespace CarRentalApi.Modules.Employees.Application.UseCases;

/// <summary>
/// Use case: Set or change admin rights (ST-3).
///
/// Flow:
/// 1) Load employee aggregate (tracked)
/// 2) Apply domain operation (SetAdminRights)
/// 3) Commit via UnitOfWork
///
/// Notes:
/// - Rights are represented as an int bitmask (flags).
/// - Validation of the bitmask (allowed bits) should live in the domain.
/// </summary>
public sealed class EmployeeUcSetAdminRights(
   IEmployeeRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<EmployeeUcSetAdminRights> _logger
) {

   public async Task<Result> ExecuteAsync(
      Guid employeeId,
      AdminRights adminRights,
      CancellationToken ct
   ) {
      if (employeeId == Guid.Empty) {
         var fail = Result.Failure(EmployeeErrors.InvalidId);
         fail.LogIfFailure(_logger, "EmployeeUcSetAdminRights.InvalidId", new { employeeId });
         return fail;
      }

      var employee = await _repository.FindByIdAsync(employeeId, ct);
      if (employee is null) {
         var fail = Result.Failure(EmployeeErrors.NotFound);
         fail.LogIfFailure(_logger, "EmployeeUcSetAdminRights.NotFound", new { employeeId });
         return fail;
      }

      var result = employee.SetAdminRights(adminRights);

      if (result.IsFailure) {
         result.LogIfFailure(_logger, "EmployeeUcSetAdminRights.DomainRejected",
            new { employeeId, adminRights });
         return result;
      }

      await _unitOfWork.SaveAllChangesAsync("Employee admin rights updated", ct);

      _logger.LogInformation("EmployeeUcSetAdminRights done employeeId={employeeId}", employeeId);
      return Result.Success();
   }
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Hinweise (für alle EmployeeUc*)
 * =====================================================================
 *
 * Was sind die konkreten UseCases (EmployeeUcCreate/Deactivate/SetAdminRights)?
 * --------------------------------------------------------------------------
 * Das sind die eigentlichen Implementierungen der WRITE-Anwendungsfälle
 * im Employees-Bounded-Context. Jeder UseCase:
 * - lädt Aggregate über ein Repository (Tracking)
 * - ruft eine Domain-Operation auf (reine Fachlogik)
 * - persistiert über UnitOfWork (ein Commit)
 *
 *
 * Warum drei getrennte Klassen?
 * -----------------------------
 * - Single Responsibility: jeder UseCase hat genau einen Zweck
 * - bessere Testbarkeit (pro UseCase eigene Unit-Tests)
 * - saubere Struktur der Application Layer
 *
 *
 * Logging-Policy:
 * --------------
 * - Fachliche Fehler (Result-Failure) werden über LogIfFailure geloggt
 * - Technische Fehler (Exceptions) gehören in Middleware / globale Handler
 * - CancellationToken wird nur durchgereicht, nicht als Fehler behandelt
 *
 *
 * Abgrenzung:
 * ----------
 * - Lesen (Details/Liste/Filter) ist kein UseCase → IEmployeeReadModel
 * - Persistenzdetails (EF Core, SQL) sind nicht hier → Infrastructure
 * - Fachliche Regeln/Validierung liegen im Domain Model → Employee Aggregate
 *
 * =====================================================================
 */

