using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Cars.Domain.Errors;

/// <summary>
/// Domain-level error definitions for car-related validation and business rules.
/// </summary>
public static class CarErrors {

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Car Not Found",
         Message: "The Requested Car Does Not Exist."
      );

   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Car ReservationId",
         Message: "The Provided Car ReservationId Is Invalid."
      );

   public static readonly DomainErrors IdAlreadyExists = new(
      ErrorCode.Conflict,
      "Car.IdAlreadyExists",
      "A car with this ID already exists."
   );
   
   public static readonly DomainErrors ManufacturerIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Manufacturer Is Required",
         Message: "A Manufacturer Must Be Provided."
      );

   public static readonly DomainErrors ModelIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Model Is Required",
         Message: "A Model Must Be Provided."
      );

   public static readonly DomainErrors LicensePlateIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "License Plate Is Required",
         Message: "A License Plate Must Be Provided."
      );

   public static readonly DomainErrors InvalidLicensePlateFormat =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid License Plate Format",
         Message: "The License Plate May Only Contain Uppercase Letters, Digits, And Hyphens."
      );

   public static readonly DomainErrors LicensePlateMustBeUnique =
      new(
         ErrorCode.Conflict,
         Title: "Duplicate License Plate",
         Message: "The License Plate Must Be Unique."
      );

      
   public static readonly DomainErrors CategoryIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Car CarCategory Is Required",
         Message: "A Car CarCategory Must Be Provided."
      );

   public static readonly DomainErrors CreatedAtIsRequired =
      new(
         ErrorCode.BadRequest,
         "Creation timestamp is required",
         "The creation timestamp (createdAt) must be provided when creating a car."
      );
   
   public static readonly DomainErrors CarNotAvailable =
      new(
         ErrorCode.BadRequest,
         Title: "Car Not Available",
         Message: "The Car Is Currently Not Available."
      );

   public static readonly DomainErrors InvalidStatusTransition =
      new(
         ErrorCode.Conflict,
         Title: "Invalid Car ReservationStatus Transition",
         Message: "The Requested Car ReservationStatus Transition Is Not Allowed."
      );
}
