using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Customers.Domain.Enums;

public static class CustomerErrors {
   
   public static readonly DomainErrors FirstNameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "First Name Is Required",
         Message: "A First Name Must Be Provided."
      );
   
   public static readonly DomainErrors InvalidFirstName =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid FirstName",
         Message: "The Provided FirstName is too shot or too long (2-100 chars)."
      );

   public static readonly DomainErrors LastNameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Last Name Is Required",
         Message: "A Last Name Must Be Provided."
      );

   public static readonly DomainErrors InvalidLastName =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid LastName",
         Message: "The Provided LastName is too shot or too long (2-100 chars)."
      );
   
   public static readonly DomainErrors EmailIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Email Is Required",
         Message: "An Email Address Must Be Provided."
      );

   public static readonly DomainErrors InvalidEmail =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid email address",
         Message: "The provided email address is not valid."
      );
   
}