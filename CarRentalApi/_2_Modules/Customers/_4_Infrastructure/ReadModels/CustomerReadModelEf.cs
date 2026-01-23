using CarRentalApi._2_Modules.Customers._1_Ports.Inbound;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.ReadModels;
using CarRentalApi._2_Modules.Customers._2_Application.Mappings;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Customers._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks._4_Infrastructure.ReadModel;
using CarRentalApi.BuildingBlocks;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi._2_Modules.Customers._4_Infrastructure.ReadModels;

public sealed class CustomerReadModelEf(
   CarRentalDbContext _dbContext
) : ICustomerReadModel {

   public async Task<Result<CustomerDetailDto>> FindByIdAsync(
      Guid Id,
      CancellationToken ct
   ) {

      var customer = await _dbContext.Customers
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Id == Id, ct);

      return customer is null
         ? Result<CustomerDetailDto>.Failure(CustomerErrors.NotFound)
         : Result<CustomerDetailDto>.Success(customer.ToCustomerDetailDto());
   }

   public async Task<Result<CustomerDetailDto>> FindByEmailAsync(
      string emailString,
      CancellationToken ct
   ) {
      if (string.IsNullOrWhiteSpace(emailString)) 
         return Result<CustomerDetailDto>.Failure(CustomerErrors.EmailIsRequired);
      
      var normalizedEmail = emailString.Trim().ToUpperInvariant();
      var customer = await _dbContext.Customers
          .AsNoTracking()
          .FirstOrDefaultAsync(c => c.Email.Value.ToUpperInvariant() == normalizedEmail, ct);
      
      return customer is null 
          ? Result<CustomerDetailDto>.Failure(CustomerErrors.EmailNotFound) 
          : Result<CustomerDetailDto>.Success(customer.ToCustomerDetailDto());
   }

   public async Task<Result<IReadOnlyList<CustomerDetailDto>>> SelectByNameAsync(
      string firstName,
      string lastName,
      CancellationToken ct
   ) {
      firstName = (firstName ?? "").Trim();
      lastName  = (lastName  ?? "").Trim();
      if (firstName.Length == 0 && lastName.Length == 0)
         return Result<IReadOnlyList<CustomerDetailDto>>.Failure(CustomerErrors.FirstNameIsRequired);
      
      var patternFirstName = $"%{firstName}%";
      var patternLastName = $"%{lastName}%";

      var customerDetailDtos = await _dbContext.Customers
         .AsNoTracking()
         // both parts must match &&
         .Where(c =>                   
            EF.Functions.Like(c.FirstName, patternFirstName) &&
            EF.Functions.Like(c.LastName,  patternLastName))
         // Limit to 50 results to avoid overload
         .Take(50) 
         .OrderBy(c => c.LastName)
         .ThenBy(c => c.FirstName)
         .Select(c => c.ToCustomerDetailDto())
         .ToListAsync(ct);

      return Result<IReadOnlyList<CustomerDetailDto>>.Success(customerDetailDtos);
   }

   public async Task<Result<PagedResult<CustomerListItemDto>>> FilterAsync(
      CustomerSearchFilter filter,
      PageRequest page,
      SortRequest sort,
      CancellationToken ct
   ) {
      if (filter is null) {
         return Result<PagedResult<CustomerListItemDto>>.Failure(
            new DomainErrors(
               ErrorCode.UnprocessableEntity,
               Title: "Filter Is Required",
               Message: "The Provided Filter Must Not Be Null."
            )
         );
      }
   
      // Normalize page defaults
      var pageNumber = page?.PageNumber > 0 ? page.PageNumber : 1;
      var pageSize   = page?.PageSize    > 0 ? page.PageSize    : 20;
      var skip       = (pageNumber - 1) * pageSize;
   
      IQueryable<Customer> query = _dbContext.Customers.AsNoTracking();
   
      // Filters
      if (!string.IsNullOrWhiteSpace(filter.Email)) {
         var email = filter.Email.Trim().ToUpperInvariant();
         query = query.Where(c => c.Email.Value.ToUpperInvariant() == email);
      }
   
      if (!string.IsNullOrWhiteSpace(filter.FirstName)) {
         var fn = filter.FirstName.Trim().ToUpperInvariant();
         query = query.Where(c => c.FirstName.ToUpperInvariant().Contains(fn));
      }
   
      if (!string.IsNullOrWhiteSpace(filter.LastName)) {
         var ln = filter.LastName.Trim().ToUpperInvariant();
         query = query.Where(c => c.LastName.ToUpperInvariant().Contains(ln));
      }
   
      // Total BEFORE paging
      var total = await query.CountAsync(ct);
   
      // Sorting (fallback: LastName, FirstName)
      query = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);
   
      // Paging + projection
      var items = await query
         .Skip(skip)
         .Take(pageSize)
         .Select(c => c.ToCustomerListItemDto())
         .ToListAsync(ct);
   
      // Wrap into PagedResult (adjust if your PagedResult has a different constructor/factory)
      var paged = new PagedResult<CustomerListItemDto>(
         items,
         total,
         pageNumber,
         pageSize
      );
   
      return Result<PagedResult<CustomerListItemDto>>.Success(paged);
   }
   
   
   // public async Task<Result<PagedResult<CustomerListItemDto>>> FilterAsync(
   //    CustomerSearchFilter filter,
   //    PageRequest page,
   //    SortRequest sort,
   //    CancellationToken ct
   // ) {
   //    if (filter is null) {
   //       return Result<PagedResult<CustomerListItemDto>>.Failure(
   //          new DomainErrors(
   //             ErrorCode.UnprocessableEntity,
   //             Title: "Filter Is Required",
   //             Message: "The Provided Filter Must Not Be Null."
   //          )
   //       );
   //    }
   //
   //    // Base read-only query
   //    IQueryable<Customer> query = _dbContext.Customers.AsNoTracking();
   //
   //    // Dynamic query composition - keep it provider-friendly.
   //    if (!string.IsNullOrWhiteSpace(filter.Email)) {
   //       var email = filter.Email.Trim().ToUpperInvariant();
   //       query = query.Where(c => c.Email.Value.ToUpper() == email);
   //    }
   //
   //    if (!string.IsNullOrWhiteSpace(filter.FirstName)) {
   //       var fn = filter.FirstName.Trim().ToUpperInvariant();
   //       query = query.Where(c => c.FirstName.ToUpper().Contains(fn));
   //    }
   //
   //    if (!string.IsNullOrWhiteSpace(filter.LastName)) {
   //       var ln = filter.LastName.Trim().ToUpperInvariant();
   //       query = query.Where(c => c.LastName.ToUpper().Contains(ln));
   //    }
   //
   //    query = query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);
   //
   //    var customers = await query
   //       .ToListAsync(ct);
   //
   //    var dtos = customers
   //       .Select(c => c.ToCustomerListItemDto())
   //       .ToList();
   //
   //    return Result<PagedResult<CustomerListItemDto>>.Success(dtos);
   // }
}
