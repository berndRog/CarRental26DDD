using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi.Data.Database;
using CarRentalApi.Domain;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Cars.Infrastructure.Repositories;

public sealed class CustomerRepositoryEf(
   CarRentalDbContext _dbContext
) : ICustomerRepository {
   public async Task<Customer?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   ) => await _dbContext.Customers
      .FirstOrDefaultAsync(x => x.Id == id, ct);

   public Task<Customer?> FindByIdentitySubjectAsync(
      IdentitySubject subject, 
      CancellationToken ct
   ) {
      throw new NotImplementedException();
   }

   public void Add(Customer customer) =>
      _dbContext.Customers.Add(customer);
}