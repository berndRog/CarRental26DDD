using CarRentalApi.Modules.Employees.Domain.Aggregates;
namespace CarRentalApi.Modules.Employees.Infrastructure;

public interface IEmployeeRepository {

   // Queries 0..1
   Task<Employee?> FindByIdAsync(
      Guid id, 
      CancellationToken ct
   );
   Task<Employee?> FindByPersonnelNumberAsync(
      string personnelNumber,
      CancellationToken ct
   );

   // Queries 0..n
   // Admins sind Employees mit AdminRights != None
   Task<IReadOnlyList<Employee>> SelectAdminsAsync(CancellationToken ct);

   // Commands
   void Add(Employee employee);
}
