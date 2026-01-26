using CarRentalApi._2_Modules.Customers._1_Ports.Inbound;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.ReadModels;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;
using CarRentalApi._2_Modules.Customers._2_Application.Mappings;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Customers._3_Domain.Errors;
using CarRentalApi._3_Infrastructure.Persistence.Database;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks._4_Infrastructure.ReadModel;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi._2_Modules.Customers._4_Infrastructure.ReadModels;

public sealed class CustomerReadModelEf(
   IIdentityGateway _identityGateway,
   CarRentalDbContext _dbContext
) : ICustomerReadModel {
   
   public async Task<CustomerDetailDto?> FindProfileAsync(CancellationToken ct) {
      
      // 1) Subject aus Gateway
      var subjectResult = IdentitySubject.Create(_identityGateway.Subject);
      if (subjectResult.IsFailure)
         return null; // oder Exception, je nach Stil

      var subject = subjectResult.Value;

      // 2) Customer laden (NO tracking, read-only)
      return await _dbContext.Customers
         .AsNoTracking()
         .Where(c => c.Subject.Value == subject.Value)
         .Select(c => c.ToCustomerDetailDto())
         .SingleOrDefaultAsync(ct);
   }
   
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
      string firstname,
      string lastname,
      CancellationToken ct
   ) {
      firstname = (firstname ?? "").Trim();
      lastname  = (lastname  ?? "").Trim();
      if (firstname.Length == 0 && lastname.Length == 0)
         return Result<IReadOnlyList<CustomerDetailDto>>.Failure(CustomerErrors.FirstnameIsRequired);
      
      var patternFirstname = $"%{firstname}%";
      var patternLastname = $"%{lastname}%";

      var customerDetailDtos = await _dbContext.Customers
         .AsNoTracking()
         // both parts must match &&
         .Where(c =>                   
            EF.Functions.Like(c.Firstname, patternFirstname) &&
            EF.Functions.Like(c.Lastname,  patternLastname))
         // Limit to 50 results to avoid overload
         .Take(50) 
         .OrderBy(c => c.Lastname)
         .ThenBy(c => c.Firstname)
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
   
      if (!string.IsNullOrWhiteSpace(filter.Firstname)) {
         var fn = filter.Firstname.Trim().ToUpperInvariant();
         query = query.Where(c => c.Firstname.ToUpperInvariant().Contains(fn));
      }
   
      if (!string.IsNullOrWhiteSpace(filter.Lastname)) {
         var ln = filter.Lastname.Trim().ToUpperInvariant();
         query = query.Where(c => c.Lastname.ToUpperInvariant().Contains(ln));
      }
   
      // Total BEFORE paging
      var total = await query.CountAsync(ct);
   
      // Sorting (fallback: Lastname, Firstname)
      query = query.OrderBy(c => c.Lastname).ThenBy(c => c.Firstname);
   
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
}
