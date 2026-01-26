using CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
namespace CarRentalApi._2_Modules.Customers._2_Application.Mappings;

public static class CustomerUseCasesMappings {

   public static CustomerProfileDto ToCustomerProfileDto(this Customer customer) => new(
      Firstname: customer.Firstname,
      Lastname: customer.Lastname,
      EmailString: customer.Email.Value,
      Street: customer.Address?.Street,
      PostalCode: customer.Address?.PostalCode,
      City: customer.Address?.City,
      Country: customer.Address?.Country
   );
}
