using CarRentalApi._2_Modules.Employees._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Employees._3_Domain.Enums;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Employees.Domain;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi._2_Modules.Employees._4_Infrastructure.Repositories;

public sealed class EmployeeRepositoryEf(
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

   public Task<bool> ExistsPersonnelNumberAsync(string personnelNumber, CancellationToken ct) {
      throw new NotImplementedException();
   }

   public Task<bool> ExistsEmailAsync(string email, CancellationToken ct) {
      throw new NotImplementedException();
   }

   public async Task<IReadOnlyList<Employee>> SelectAdminsAsync(CancellationToken ct) =>
      await _dbContext.Employees
         .AsNoTracking()
         .Where(e => e.AdminRights != AdminRights.None)
         .OrderBy(e => e.LastName)
         .ToListAsync(ct);

   public void Add(Employee employee) =>
      _dbContext.Employees.Add(employee);
}