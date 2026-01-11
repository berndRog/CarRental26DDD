using CarRentalApi.Modules.Customers.Application.contracts.Dto;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Customers.Application.Contracts.Mapping;

public static class CustomerMapper {
   
   public static CustomerContractDto ToCustomerDto(this Customer customer) => new(
      Id: customer.Id.ToString(),
      Identity: customer.Identity,
      customer.CreatedAt,
      customer.IsBlocked
   );
}
