using CarRentalApi.Modules.Customers.Application.ReadModel.Dto;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Customers.Application.Contracts.Mapping;

public static class CustomerMapperReadModel {
   
   public static CustomerDetails ToCustomerDetails(this Customer customer) => new(
      Id: customer.Id,
      FirstName: customer.FirstName,
      LastName: customer.LastName,
      Email: customer.Email,
      CreatedAt: customer.CreatedAt,
      Address: customer.Address, 
      IsBlocked: customer.IsBlocked
   );
}
