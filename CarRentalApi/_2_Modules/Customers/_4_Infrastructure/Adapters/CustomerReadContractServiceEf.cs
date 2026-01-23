using CarRentalApi._2_Modules.Customers._1_Ports.Inbound;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.Contracts;
using CarRentalApi._2_Modules.Customers._2_Application.Mappings;
using CarRentalApi.Data.Database;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi._2_Modules.Customers._4_Infrastructure.Adapters;

/// <summary>
/// Read-only EF Core implementation of <see cref="ICustomerReadContract"/>.
/// </summary>
public sealed class CustomerReadContractServiceEf(
   CarRentalDbContext _dbContext
) : ICustomerReadContract {
   
   public async Task<CustomerContractDto?> FindByIdAsync(
      Guid customerId,
      CancellationToken ct
   ) {
      if (customerId == Guid.Empty) return null;

      // Read-only query: no tracking, DTO projection only
      var customer = await _dbContext.Customers
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Id == customerId, ct);

      return customer?.ToCustomerContractDto();
   }

   public async Task<CustomerContractDto?> FindByEmailAsync(
      string emailString,
      CancellationToken ct
   ) {
      if (string.IsNullOrWhiteSpace(emailString))
         return null;

      var normalizedEmail = emailString.Trim().ToUpperInvariant();
      var customer = await _dbContext.Customers
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Email.Value.ToUpperInvariant() == normalizedEmail, ct);

      return customer?.ToCustomerContractDto();
   }

   public async Task<IReadOnlyList<CustomerContractDto>> SelectByNameAsync(
      string firstName,
      string lastName,
      CancellationToken ct
   ) {
      firstName = (firstName ?? "").Trim();
      lastName = (lastName ?? "").Trim();
      if (firstName.Length == 0 && lastName.Length == 0)
         return Array.Empty<CustomerContractDto>();

      var patternFirstName = $"%{firstName}%";
      var patternLastName = $"%{lastName}%";

      var customerContractDtos = await _dbContext.Customers
         .AsNoTracking()
         // both parts must match &&
         .Where(c =>
            EF.Functions.Like(c.FirstName, patternFirstName) &&
            EF.Functions.Like(c.LastName, patternLastName))
         // Limit to 50 results to avoid overload
         .Take(50)
         .OrderBy(c => c.LastName)
         .ThenBy(c => c.FirstName)
         .Select(c => c.ToCustomerContractDto())
         .ToListAsync(ct);

      return customerContractDtos;
   }
}