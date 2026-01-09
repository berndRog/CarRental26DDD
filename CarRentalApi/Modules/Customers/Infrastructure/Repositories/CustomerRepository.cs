using CarRentalApi.Data.Database;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Cars.Infrastructure.Repositories;

public sealed class CustomerRepository(
   CarRentalDbContext _dbContext
) : ICustomerRepository {
  
   public async Task<Customer?> FindByIdAsync(Guid id, CancellationToken ct)
      => await _dbContext.Customers
         .FirstOrDefaultAsync(x => x.Id == id, ct);
   
   public void Add(Customer customer) =>
      _dbContext.Customers.Add(customer);
}