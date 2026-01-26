using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._2_Modules.Customers._3_Domain.Errors;

public static class CustomerErrors {
   
   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid customer id",
         Message: "The provided customer id is invalid."
      );
   
   public static readonly DomainErrors FirstnameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "First name required",
         Message: "A first name must be provided."
      );

   public static readonly DomainErrors InvalidFirstname =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid first name",
         Message: "The provided first name is too short or too long (2–100 characters)."
      );

   public static readonly DomainErrors LastnameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Last name required",
         Message: "A last name must be provided."
      );

   public static readonly DomainErrors InvalidLastname =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid last name",
         Message: "The provided last name is too short or too long (2–100 characters)."
      );

   public static readonly DomainErrors EmailIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Email required",
         Message: "An email address must be provided."
      );
   
   public static readonly DomainErrors CreatedAtIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Creation Timestamp Required",
         Message: "The creation timestamp (createdAt) must be provided."
      );

   public static readonly DomainErrors EmailNotFound =
      new(
         ErrorCode.NotFound,
         Title: "Customer not found",
         Message: "No customer with the given email address exists."
      );
   
   public static readonly DomainErrors UserNameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Preferred Username required",
         Message: "A Username must be provided."
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

   public static readonly DomainErrors NameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Name is required",
         Message: "The name parameter is required and cannot be empty."
      );
   
}