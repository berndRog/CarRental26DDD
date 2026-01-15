using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;

namespace CarRentalApi.Modules.Customers.Domain.Errors;

public static class CustomerErrors {

   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Invalid customer id",
         Message: "The provided customer id is invalid."
      );
   
      public static readonly DomainErrors EmailNotFound =
      new(
         ErrorCode.NotFound,
         Title: "Customer not found",
         Message: "No customer with the given email address exists."
      );
   
   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Customer not found",
         Message: "The requested customer does not exist."
      );
   
   public static readonly DomainErrors AlreadyBlocked =
      new(
         ErrorCode.Conflict,
         Title: "Customer already blocked",
         Message: "The customer is already blocked and cannot be blocked again."
      );
   
}
