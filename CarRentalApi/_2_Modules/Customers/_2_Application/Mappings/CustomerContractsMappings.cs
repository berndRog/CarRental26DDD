using CarRentalApi._2_Modules.Customers._2_Application.Dtos.Contracts;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.ReadModels;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
namespace CarRentalApi._2_Modules.Customers._2_Application.Mappings;

public static class CustomerContractsMappings {

   public static CustomerContractDto ToCustomerContractDto(this Customer customer) => new(
      Id: customer.Id,
      Firstname: customer.Firstname,
      Lastname: customer.Lastname,
      Email: customer.Email
   );
}
