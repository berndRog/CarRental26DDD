using CarRentalApi.Modules.Customers.Application.contracts.Dto;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Customers.Application.Contracts.Mapping;

public static class CustomerMapper {
   
   public static CustomerDto ToDto(this Customer customer) => new(
      customer.Id.ToString(),
      customer.FirstName,
      customer.LastName,
      customer.Email,
      customer.CreatedAt,
      customer.Address?.Street,
      customer.Address?.PostalCode,
      customer.Address?.City
   );
}
