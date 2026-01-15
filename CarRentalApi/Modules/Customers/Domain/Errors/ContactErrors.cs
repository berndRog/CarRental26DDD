using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;

public static class ContactErrors {

   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid contact ID",
         Message: "The provided contact ID is invalid."
      );
   
   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Contact not found",
         Message: "The requested contact does not exist."
      );

   public static readonly DomainErrors AlreadyBlocked =
      new(
         ErrorCode.Conflict,
         Title: "Customer already blocked",
         Message: "The customer is already blocked and cannot be blocked again."
      );
}
