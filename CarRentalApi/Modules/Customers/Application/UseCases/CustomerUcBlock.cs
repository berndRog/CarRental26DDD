using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Errors;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Domain;
namespace CarRentalApi.Modules.Customers.Application.UseCases.Block;

public sealed class CustomerUcBlock(
   ICustomerRepository _customerRepository,
   IUnitOfWork _unitOfWork,
   ILogger<CustomerUcBlock> _logger
)  {

   public async Task<Result> ExecuteAsync(
      Guid customerId,
      DateTimeOffset blockedAt,
      CancellationToken ct
   ) {
      _logger.LogInformation(
         "CustomerUcBlock start customerId={customerId}",
         customerId
      );

      // 1) Load aggregate with tracking (we want to modify it)
      var customer = await _customerRepository.FindByIdAsync(customerId, ct: ct);

      if (customer is null) 
         return Result.Failure(DomainErrors.NotFound);

      // 2) Domain behavior
      var blockResult = customer.Block(blockedAt);

      if (blockResult.IsFailure) {
         blockResult.LogIfFailure(_logger, "CustomerUcBlock block failed", new { customerId });
         return blockResult;
      }

      // 3) Persist changes (throws exception on failure)
      var saved = await _unitOfWork.SaveAllChangesAsync("CustomerUcBlock",ct);
      return Result.Success();
   }
}
