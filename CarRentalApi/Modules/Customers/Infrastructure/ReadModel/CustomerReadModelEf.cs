using CarRentalApi.BuildingBlocks;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Customers.Application.Contracts.Mapping;
using CarRentalApi.Modules.Customers.Application.ReadModel;
using CarRentalApi.Modules.Customers.Application.ReadModel.Dto;
using CarRentalApi.Modules.Customers.Domain.Errors;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Customers.Infrastructure.ReadModel;

public sealed class CustomerReadModelEf(
   CarRentalDbContext _dbContext
) : ICustomerReadModel {
   
   public async Task<Result<CustomerDetail>> FindByIdAsync(
      Guid Id,
      CancellationToken ct
   ) {
      
      var customer = await _dbContext.Customers
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Id == Id, ct);
         
      return customer is null 
         ? Result<CustomerDetail>.Failure(CustomerErrors.NotFound) 
         : Result<CustomerDetail>.Success(customer.ToCustomerDetail());
   }
   /*
   public async Task<Result<CustomerDetail>> FindByEmailAsync(
      string email,
      CancellationToken ct
   ) {
      if (string.IsNullOrWhiteSpace(email)) 
         return Result<CustomerDetail>.Failure(CustomerErrors.EmailIsRequired);

      var normalizedEmail = email.Trim().ToUpperInvariant();
      var customer = await _dbContext.Customers
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Email.ToUpper() == normalizedEmail, ct);
      
      return customer is null 
         ? Result<CustomerDetail>.Failure(CustomerErrors.EmailNotFound) 
         : Result<CustomerDetail>.Success(customer.ToCustomerDetail());
   }

   public Task<Result<IReadOnlyList<CustomerListItem>>> SelectByNameAsync(string firstName, string lastName, CancellationToken ct) {
      throw new NotImplementedException();
   }

   public Task<Result<PagedResult<CustomerListItem>>> FilterAsync(CustomerSearchFilter filter, PageRequest page, SortRequest sort, CancellationToken ct) {
      throw new NotImplementedException();
   }

   public async Task<Result<IReadOnlyList<CustomerListItem>>> SelectByNameAsync(
      string name,
      CancellationToken ct
   ) {
      if (string.IsNullOrWhiteSpace(name))
         return Result<IReadOnlyList<CustomerListItem>>.Failure(CustomerErrors.FirstNameIsRequired);

      var searchPattern = $"%{name.Trim()}%";

      var customers = await _dbContext.Customers
         .AsNoTracking()
         .Where(c => EF.Functions.Like(c.FirstName + " " + c.LastName, searchPattern))
         .OrderBy(c => c.LastName)
         .ThenBy(c => c.FirstName)
         .ToListAsync(ct);

      var customerListItems = customers
         .Select(c => c.ToCustomerListItem())
         .ToList();

      return Result<IReadOnlyList<CustomerListItem>>.Success(customerListItems);
   }
*/

}
