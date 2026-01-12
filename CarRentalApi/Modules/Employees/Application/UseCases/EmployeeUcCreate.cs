using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
using CarRentalApi.Modules.Employees.Domain;
using CarRentalApi.Modules.Employees.Domain.Aggregates;
using CarRentalApi.Modules.Employees.Domain.Enums;
using CarRentalApi.Modules.Employees.Domain.Errors;
using CarRentalApi.Modules.Employees.Infrastructure;

namespace CarRentalApi.Modules.Employees.Application.UseCases;

/// <summary>
/// Use case: Create a new employee (EM-1).
///
/// Flow:
/// 1) Validate basic inputs (use-case guards)
/// 2) Check uniqueness constraints (personnel number / email)
/// 3) Create domain aggregate (Employee.Create)
/// 4) Add to repository + commit via UnitOfWork
///
/// Logging:
/// - Uses LogIfFailure for all business failures (Result-based)
/// - Does not handle technical exceptions (middleware responsibility)
/// </summary>
public sealed class EmployeeUcCreate(
   IEmployeeRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<EmployeeUcCreate> _logger
) {
   public async Task<Result<Guid>> ExecuteAsync(
      string firstName,
      string lastName,
      string email,
      string personnelNumber,
      AdminRights adminRights,
      DateTimeOffset createdAt = default,
      string? id = null,
      Address? address = null,
      CancellationToken ct = default
   ) {

      // ---- Use-case guards (cheap validations) ----
      if (string.IsNullOrWhiteSpace(personnelNumber)) 
         return Result<Guid>.Failure(EmployeeErrors.PersonnelNumberIsRequired);
      
      if (string.IsNullOrWhiteSpace(email)) 
         return Result<Guid>.Failure(EmployeeErrors.EmailIsRequired);
      
      // ---- Uniqueness checks (I/O) ----
      if (await _repository.ExistsPersonnelNumberAsync(personnelNumber, ct)) {
         var fail = Result<Guid>.Failure(EmployeeErrors.PersonnelNumberMustBeUnique);
         fail.LogIfFailure(_logger, "EmployeeUcCreate.PersonnelNumberMustBeUnique", new { personnelNumber });
         return fail;
      }

      if (await _repository.ExistsEmailAsync(email, ct)) {
         var fail = Result<Guid>.Failure(EmployeeErrors.EmailMustBeUnique);
         fail.LogIfFailure(_logger, "EmployeeUcCreate.EmailMustBeUnique", new { email });
         return fail;
      }

      // ---- Domain factory (invariants) ----
      var createResult = Employee.Create(
         firstName: firstName,
         lastName: lastName,
         email: email,
         personnelNumber: personnelNumber,
         adminRights: adminRights,
         createdAt: createdAt,
         id: id,
         address: address
      );

      if (createResult.IsFailure) {
         createResult.LogIfFailure(
            _logger, "EmployeeUcCreate.DomainRejected",
            new { firstName, lastName, email, personnelNumber, adminRights });
         return Result<Guid>.Failure(createResult.Error);
      }
      var employee = createResult.Value!;

      // Add to repository
      _repository.Add(employee);
      
      // Persist via UnitOfWork
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Employee created", ct);

      _logger.LogInformation(
         "EmployeeUcCreate done Id={id} personnelNumber={nr} savedRows={rows}",
         employee.Id, employee.PersonnelNumber, savedRows);

      return Result<Guid>.Success(employee.Id);
   }
}
