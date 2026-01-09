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
         ErrorCode.BadReqest,
         Title: "Invalid Car Id",
         Message: "The Provided Car Id Is Invalid."
      );

   public static readonly DomainErrors CategoryIsRequired =
      new(
         ErrorCode.BadReqest,
         Title: "Car Category Is Required",
         Message: "A Car Category Must Be Provided."
      );

   public static readonly DomainErrors ManufacturerIsRequired =
      new(
         ErrorCode.BadReqest,
         Title: "Manufacturer Is Required",
         Message: "A Manufacturer Must Be Provided."
      );

   public static readonly DomainErrors ModelIsRequired =
      new(
         ErrorCode.BadReqest,
         Title: "Model Is Required",
         Message: "A Model Must Be Provided."
      );

   public static readonly DomainErrors LicensePlateIsRequired =
      new(
         ErrorCode.BadReqest,
         Title: "License Plate Is Required",
         Message: "A License Plate Must Be Provided."
      );

   public static readonly DomainErrors InvalidLicensePlateFormat =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid License Plate Format",
         Message: "The License Plate May Only Contain Uppercase Letters, Digits, And Hyphens."
      );

   public static readonly DomainErrors LicensePlateMustBeUnique =
      new(
         ErrorCode.Conflict,
         Title: "Duplicate License Plate",
         Message: "The License Plate Must Be Unique."
      );

   public static readonly DomainErrors CarNotAvailable =
      new(
         ErrorCode.BadReqest,
         Title: "Car Not Available",
         Message: "The Car Is Currently Not Available."
      );

   public static readonly DomainErrors InvalidStatusTransition =
      new(
         ErrorCode.Conflict,
         Title: "Invalid Car Status Transition",
         Message: "The Requested Car Status Transition Is Not Allowed."
      );
}
