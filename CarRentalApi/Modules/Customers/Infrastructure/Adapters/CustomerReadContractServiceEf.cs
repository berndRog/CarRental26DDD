using CarRentalApi.BuildingBlocks;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Customers.Application.Contracts;
using CarRentalApi.Modules.Customers.Application.contracts.Dto;
using CarRentalApi.Modules.Customers.Application.Contracts.Mapping;
using CarRentalApi.Modules.Customers.Domain.Errors;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Customers.Application.Services;

/// <summary>
/// Read-only EF Core implementation of <see cref="ICustomerReadContract"/>.
/// </summary>
public sealed class CustomerReadContractServiceEf(
   CarRentalDbContext _dbContext
) : ICustomerReadContract {

   public async Task<Result<CustomerContractDto>> FindByIdAsync(
      Guid customerId,
      CancellationToken ct
   ) {
      if (customerId == Guid.Empty) 
         return Result<CustomerContractDto>.Failure(CustomerErrors.InvalidId);
      
      // Read-only query: no tracking, DTO projection only
      var customer = await _dbContext.Customers
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Id == customerId, ct);
      
      return customer is null 
         ? Result<CustomerContractDto>.Failure(CustomerErrors.NotFound) 
         : Result<CustomerContractDto>.Success(customer.ToCustomerDto());
   }
   
   /*
   public async Task<Result<IReadOnlyList<CustomerDto>>> FilterAsync(
      CustomerFilter filter,
      CancellationToken ct
   ) {
      // If you want a domain error for null filters, create CustomerErrors.FilterIsRequired.
      if (filter is null) {
         return Result<IReadOnlyList<CustomerDto>>.Failure(
            new DomainErrors(
               ErrorCode.UnprocessableEntity,
               Title: "Filter Is Required",
               Message: "The Provided Filter Must Not Be Null."
            )
         );
      }

      // Base read-only query
      IQueryable<Customer> query = _dbContext.Customers.AsNoTracking();

      // Dynamic query composition - keep it provider-friendly.
      if (!string.IsNullOrWhiteSpace(filter.Email)) {
         var email = filter.Email.Trim().ToUpperInvariant();
         query = query.Where(c => c.Email.ToUpper() == email);
      }

      if (!string.IsNullOrWhiteSpace(filter.FirstName)) {
         var fn = filter.FirstName.Trim().ToUpperInvariant();
         query = query.Where(c => c.FirstName.ToUpper().Contains(fn));
      }

      if (!string.IsNullOrWhiteSpace(filter.LastName)) {
         var ln = filter.LastName.Trim().ToUpperInvariant();
         query = query.Where(c => c.LastName.ToUpper().Contains(ln));
      }
      
      query = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);

      var customers = await query
         .ToListAsync(ct);
      
      var dtos = customers
         .Select(c => c.ToCustomerDto())
         .ToList();

      return Result<IReadOnlyList<CustomerDto>>.Success(dtos);
   }
   */
}
