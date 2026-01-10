using CarRentalApi.BuildingBlocks;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Customers.Application.ReadModel;
using CarRentalApi.Modules.Customers.Application.ReadModel.Dto;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Customers.Infrastructure.ReadModel;

public sealed class CustomerReadModel(
   CarRentalDbContext _dbContext
) : ICustomerReadModel {
   
   public async Task<Result<CustomerDetails>> FindByIdAsync(
      Guid customerId,
      CancellationToken ct
   ) {
      var dto = await _dbContext.Customers
         .AsNoTracking()
         .Where(c => c.Id == customerId)
         .Select(c => c.ToCustomerDetails())
         .SingleOrDefaultAsync(ct);

      return dto is null
         ? Result.NotFound<CustomerDetails>()
         : Result.Success(dto);
   }
}
