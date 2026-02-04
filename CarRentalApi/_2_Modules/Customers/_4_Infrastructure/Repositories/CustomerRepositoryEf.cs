using CarRentalApi._2_Modules.Customers._1_Ports.Outbound;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._3_Infrastructure.Persistence.Database;
using CarRentalApi._4_BuildingBlocks._3_Domain;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
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
      string subject,
      bool noTracking = true,
      CancellationToken ct = default
   ) {
      var query = _dbContext.Customers as IQueryable<Customer>;
      if (noTracking)
         query = query.AsNoTracking();

      return query
         .FirstOrDefaultAsync(c => c.Subject == subject, ct);
   }


   public async Task<Customer?> FindByEmailAsync(
      string email,
      CancellationToken ct
   ) {
      var customer = await _dbContext.Customers
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Email == email, ct);
      return customer;
   }

   
   public void Add(Customer customer) =>
      _dbContext.Customers.Add(customer);
}



