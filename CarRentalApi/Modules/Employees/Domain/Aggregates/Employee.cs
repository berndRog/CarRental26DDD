using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.Modules.Common.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
using CarRentalApi.Modules.Employees.Domain.Enums;
using CarRentalApi.Modules.Employees.Domain.Errors;
namespace CarRentalApi.Modules.Employees.Domain.Aggregates;

public class Employee: Person {
   
   public string PersonnelNumber { get; protected set; } = string.Empty;
   public AdminRights AdminRights { get; private set; } = AdminRights.ViewReports;

   // EF Core ctor
   protected Employee() { }

   // Domain ctor
   protected Employee(
      Guid id,
      string firstName,
      string lastName,
      string email,
      string personnelNumber,
      AdminRights adminRights,
      Address? address = null
   ) : base(id, firstName, lastName, email, address) {
      PersonnelNumber = personnelNumber;
      AdminRights = adminRights;
   }

   // ---------- Factory (Result-based) ----------
   public static Result<Employee> Create(
      string firstName,
      string lastName,
      string email,
      string personnelNumber,
      AdminRights adminRights = AdminRights.None,
      string? id = null,
      Address? address = null
   ) {
      // Normalize input early
      firstName = firstName?.Trim() ?? string.Empty;
      lastName = lastName?.Trim() ?? string.Empty;
      email = email?.Trim() ?? string.Empty;
      personnelNumber = personnelNumber?.Trim() ?? string.Empty;
      
      var baseValidation = ValidatePersonData(firstName, lastName, email);
      if (baseValidation.IsFailure)
         return Result<Employee>.Failure(baseValidation.Error);

      if (string.IsNullOrWhiteSpace(personnelNumber))
         return Result<Employee>.Failure(EmployeeErrors.PersonnelNumberIsRequired);

      var result = EntityId.Resolve(id, PersonErrors.InvalidId);
      if (result.IsFailure)
         return Result<Employee>.Failure(result.Error);
      var employeeId = result.Value;
      
      var employee = new Employee(
         employeeId,
         firstName,
         lastName,
         email,
         personnelNumber,
         adminRights,
         address
      );

      return Result<Employee>.Success(employee);
   }
   
}