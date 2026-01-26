using CarRentalApi._2_Modules.Employees._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Employees._3_Domain.Enums;
using CarRentalApi._2_Modules.Employees._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi._4_BuildingBlocks.Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.Modules.Employees.Domain;
namespace CarRentalApi._2_Modules.Employees._2_Application.UseCases;

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
      string firstname,
      string lastname,
      string emailString,
      string phoneString,
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
      
      if (string.IsNullOrWhiteSpace(emailString)) 
         return Result<Guid>.Failure(EmployeeErrors.EmailIsRequired);
      
      // ---- Uniqueness checks (I/O) ----
      if (await _repository.ExistsPersonnelNumberAsync(personnelNumber, ct)) {
         var fail = Result<Guid>.Failure(EmployeeErrors.PersonnelNumberMustBeUnique);
         fail.LogIfFailure(_logger, "EmployeeUcCreate.PersonnelNumberMustBeUnique", new { personnelNumber });
         return fail;
      }

      if (await _repository.ExistsEmailAsync(emailString, ct)) {
         var fail = Result<Guid>.Failure(EmployeeErrors.EmailMustBeUnique);
         fail.LogIfFailure(_logger, "EmployeeUcCreate.EmailMustBeUnique", new { email = emailString });
         return fail;
      }

      // ---- Domain factory (invariants) ----
      var result = Employee.Create(
         firstname: firstname,
         lastname: lastname,
         emailString: emailString,
         phoneString:phoneString,
         personnelNumber: personnelNumber,
         adminRights: adminRights,
         createdAt: createdAt,
         id: id,
         address: address
      );
      if (result.IsFailure)
         return Result<Guid>.Failure(result.Error);
      
      // Add to repository
      var employee = result.Value!;
      _repository.Add(employee);
      
      // Persist via UnitOfWork
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Employee created", ct);

      _logger.LogInformation(
         "EmployeeUcCreate done Id={id} personnelNumber={nr} savedRows={rows}",
         employee.Id, employee.PersonnelNumber, savedRows);

      return Result<Guid>.Success(employee.Id);
   }
}
