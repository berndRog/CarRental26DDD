using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Customers.Application.Contracts;
using CarRentalApi.Modules.Customers.Application.contracts.Dto;
using CarRentalApi.Modules.Customers.Application.Contracts.Dto;
using CarRentalApi.Modules.Customers.Application.Contracts.Mapping;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.Errors;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Customers.Application.Services;

/// <summary>
/// Read-only EF Core implementation of <see cref="ICustomerReadApi"/>.
/// </summary>
public sealed class CustomerReadService(
   CarRentalDbContext _dbContext
) : ICustomerReadApi {

   public async Task<Result<CustomerDto>> FindByIdAsync(
      Guid customerId,
      CancellationToken ct
   ) {
      if (customerId == Guid.Empty) 
         return Result<CustomerDto>.Failure(CustomerErrors.InvalidId);
      
      // Read-only query: no tracking, DTO projection only
      var customer = await _dbContext.Customers
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Id == customerId, ct);
      
      if (customer is null) 
         return Result<CustomerDto>.Failure(CustomerErrors.NotFound);

      return Result<CustomerDto>.Success(customer.ToDto());
   }

   public async Task<Result<CustomerDto>> FindByEmailAsync(
      string email,
      CancellationToken ct
   ) {
      if (string.IsNullOrWhiteSpace(email)) {
         return Result<CustomerDto>.Failure(CustomerErrors.EmailIsRequired);
      }

      var normalizedEmail = email.Trim().ToUpperInvariant();

      var customer = await _dbContext.Customers
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Email.ToUpper() == normalizedEmail, ct);
      
      if (customer is null) 
         return Result<CustomerDto>.Failure(CustomerErrors.EmailNotFound);

      return Result<CustomerDto>.Success(customer.ToDto());
   }

   public async Task<Result<IReadOnlyList<CustomerDto>>> FindByNameAsync(
      string? firstName,
      string? lastName,
      CancellationToken ct
   ) {
      if (string.IsNullOrWhiteSpace(firstName)) 
         return Result<IReadOnlyList<CustomerDto>>.Failure(CustomerErrors.FirstNameIsRequired);
      if (string.IsNullOrWhiteSpace(lastName)) 
         return Result<IReadOnlyList<CustomerDto>>.Failure(CustomerErrors.LastNameIsRequired);

      var upperFirstName = firstName.Trim().ToUpperInvariant();
      var upperLastName = lastName.Trim().ToUpperInvariant();

      // Exact match, case-insensitive.
      // (Alternative: EF.Functions.Like for "starts with" / "contains" searches.)
      var customers = await _dbContext.Customers
         .AsNoTracking()
         .Where(c => 
            c.FirstName.ToUpper() == upperFirstName &&
            c.LastName.ToUpper() == upperLastName)
         .OrderBy(c => c.LastName)
         .ThenBy(c => c.FirstName)
         .ToListAsync(ct);
      
      // Collection query: Success even if empty.
      var dtos = customers
         .Select(c => c.ToDto())
         .ToList();
      return Result<IReadOnlyList<CustomerDto>>.Success(dtos);
   }

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
         .Select(c => c.ToDto())
         .ToList();

      return Result<IReadOnlyList<CustomerDto>>.Success(dtos);
   }
   
}
