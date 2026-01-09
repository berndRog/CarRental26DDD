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

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Customer not found",
         Message: "The requested customer does not exist."
      );

   public static readonly DomainErrors EmailIsRequired =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Email is required",
         Message: "An email address must be provided."
      );

   public static readonly DomainErrors EmailNotFound =
      new(
         ErrorCode.NotFound,
         Title: "Customer not found",
         Message: "No customer with the given email address exists."
      );

   public static readonly DomainErrors FirstNameIsRequired =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "First Name Is Required",
         Message: "A First Name Must Be Provided."
      );

   public static readonly DomainErrors LastNameIsRequired =
      new(
         ErrorCode.UnprocessableEntity,
         Title: "Last Name Is Required",
         Message: "A Last Name Must Be Provided."
      );
}
