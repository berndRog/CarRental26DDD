using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Employees.Domain;
using CarRentalApi.Modules.Employees.Domain.Aggregates;
using CarRentalApi.Modules.Employees.Domain.Errors;
using CarRentalApi.Modules.Employees.Infrastructure;

namespace CarRentalApi.Modules.Employees.Application.UseCases;

/// <summary>
/// Use case: Deactivate an employee (EM-2).
///
/// Flow:
/// 1) Check guards
/// 2) Load employee aggregate (tracked)
/// 3) Apply domain transition (Deactivate)
/// 4) Commit via UnitOfWork
///
/// Logging:
/// - Uses LogIfFailure for NotFound and domain rejection
/// </summary>
public sealed class EmployeeUcDeactivate(
   IEmployeeRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<EmployeeUcDeactivate> _logger
) {

   public async Task<Result> ExecuteAsync(
      Guid employeeId,
      DateTimeOffset deactivatedAt = default,
      CancellationToken ct = default
   ) {
       // 1) Check guards
      if (deactivatedAt == default)
         return Result.Failure(EmployeeErrors.DeactivatedAtIsRequired);
      
      if (employeeId == Guid.Empty) {
         var fail = Result.Failure(EmployeeErrors.InvalidId);
         fail.LogIfFailure(_logger, "EmployeeUcDeactivate.InvalidId", new { employeeId });
         return fail;
      }

      // 2) Load aggregate (tracked)
      var employee = await _repository.FindByIdAsync(employeeId, ct);
      if (employee is null) {
         var fail = Result.Failure(EmployeeErrors.NotFound);
         fail.LogIfFailure(_logger, "EmployeeUcDeactivate.NotFound", new { employeeId });
         return fail;
      }
      
      // 3) Apply domain transition (pure)
      var result = employee.Deactivate(deactivatedAt);
      if (result.IsFailure) {
         result.LogIfFailure(_logger, "EmployeeUcDeactivate.DomainRejected", 
            new { employeeId, deactivatedAt });
         return result;
      }

      // 4) Persist changes
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Employee deactivated", ct);
      _logger.LogInformation(
         "EmployeeUcDeactivate done employeeId={id} savedRows={rows}", 
         employeeId, savedRows);
      
      return Result.Success();
   }
}
