using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._2_Modules.Customers._3_Domain.Errors;

public static class CustomerApplicationErrors {
   

   public static readonly DomainErrors NotProvisioned =
      new(
         ErrorCode.NotFound,
         Title: "Customer is not provisioned",
         Message: "No customer with the given sub exists."
      );
   

   public static readonly DomainErrors EmployeesCannotUpdateCustomerProfile =
      new(
         ErrorCode.Conflict,
         Title: "Employee cannot update Customer profiles",
         Message: "The customer profile is blocked against employees access."
      );
   
   
}