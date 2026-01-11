using CarRentalApi.Modules.Customers.Application.contracts.Dto;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Customers.Application.Contracts.Mapping;

public static class CustomerMapper {
   
   public static CustomerDto ToCustomerDto(this Customer customer) => new(
      Id: customer.Id.ToString(),
      FirstName: customer.FirstName,
      LastName: customer.LastName,
      Email: customer.Email,
      customer.CreatedAt,
      customer.IsBlocked,
      customer.Address
   );
}
