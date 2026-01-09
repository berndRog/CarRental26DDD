using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Employees.Domain.Aggregates;
using CarRentalApi.Modules.Employees.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Employees.Infrastructure.Repositories;

public sealed class EmployeeRepository(
   CarRentalDbContext _dbContext
) : IEmployeeRepository {

   public async Task<Employee?> FindByIdAsync(
      Guid id, 
      CancellationToken ct
   ) => await _dbContext.Employees
         .FirstOrDefaultAsync(e => e.Id == id, ct);

   public async Task<Employee?> FindByPersonnelNumberAsync(
      string personnelNumber,
      CancellationToken ct
   ) => await _dbContext.Employees
      .FirstOrDefaultAsync(e => e.PersonnelNumber == personnelNumber, ct);
         
   public async Task<IReadOnlyList<Employee>> SelectAdminsAsync(CancellationToken ct) =>
      await _dbContext.Employees
         .AsNoTracking()
         .Where(e => e.AdminRights != AdminRights.None)
         .OrderBy(e => e.LastName)
         .ToListAsync(ct);

   public void Add(Employee employee) =>
      _dbContext.Employees.Add(employee);
}