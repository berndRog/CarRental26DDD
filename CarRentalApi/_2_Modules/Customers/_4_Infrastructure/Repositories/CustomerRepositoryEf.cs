using CarRentalApi._2_Modules.Customers._1_Ports.Outbound;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._3_Infrastructure.Persistence.Database;
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
      IdentitySubject subject,
      CancellationToken ct
   ) => 
      _dbContext.Customers
      //.AsNoTracking()
      .FirstOrDefaultAsync(c => c.Subject == subject, ct);

   public Task<Customer?> FindByEmailAsync(IdentitySubject subject, CancellationToken ct) {
      throw new NotImplementedException();
   }

   public async Task<Customer?> FindByEmailAsync(
      Email email,
      CancellationToken ct
   ) {
      var customer = await _dbContext.Customers
         //.AsNoTracking()
         .FirstOrDefaultAsync(c => c.Email == email, ct);
      
      return customer;
   }

   
   public void Add(Customer customer) =>
      _dbContext.Customers.Add(customer);
}



