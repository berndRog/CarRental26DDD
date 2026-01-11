using CarRentalApi.Modules.Customers.Application.ReadModel.Dto;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Customers.Application.Contracts.Mapping;

public static class CustomerReadModelMapping {
   
   public static CustomerDetail ToCustomerDetail(this Customer customer) => new(
      Id: customer.Id,
      CreatedAt: customer.CreatedAt,
      IsBlocked: customer.IsBlocked,
      BlockedAt: customer.BlockedAt
   );
   
   public static CustomerListItem ToCustomerListItem(this Customer customer) => new(
      Id: customer.Id,
      CreatedAt: customer.CreatedAt,
      IsBlocked: customer.IsBlocked
   );
}
