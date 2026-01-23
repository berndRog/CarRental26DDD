using CarRentalApi._2_Modules.Customers._2_Application.Dtos.Contracts;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.ReadModels;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
namespace CarRentalApi._2_Modules.Customers._2_Application.Mappings;

public static class CustomerReadModelsMappings {

   // Read Model Mappings 
   public static CustomerDetailDto ToCustomerDetailDto(this Customer customer) => new(
      Id: customer.Id,
      CreatedAt: customer.CreatedAt,
      IsBlocked: customer.IsBlocked,
      BlockedAt: customer.BlockedAt
   );
   
   public static CustomerListItemDto ToCustomerListItemDto(this Customer customer) => new(
      Id: customer.Id,
      CreatedAt: customer.CreatedAt,
      IsBlocked: customer.IsBlocked
   );
   
   // Use Cases Mappings
}
