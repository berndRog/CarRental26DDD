using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Rentals.Application.Errors;

/// <summary>
/// Use-case level error definitions for rental-related application flows
/// (e.g. pick-up, return).
/// </summary>
public static class RentalApplicationErrors {
   public static readonly DomainErrors ReservationNotFound =
      new(
         ErrorCode.NotFound,
         Title: "Reservation Not Found",
         Message: "The Requested Reservation Does Not Exist."
      );

   public static readonly DomainErrors CustomerNotFound =
      new(
         ErrorCode.NotFound,
         Title: "Customer Not Found",
         Message: "The Requested Customer Does Not Exist."
      );

   public static readonly DomainErrors CarNotFound =
      new(
         ErrorCode.NotFound,
         Title: "Car Not Found",
         Message: "The Requested Car Does Not Exist."
      );

   public static readonly DomainErrors RentalNotFound =
      new(
         ErrorCode.NotFound,
         Title: "Rental Not Found",
         Message: "The Requested Rental Does Not Exist."
      );

   public static readonly DomainErrors ReservationInvalidStatus =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Reservation Status",
         Message: "The Reservation Status Does Not Allow Pick-Up."
      );

   public static readonly DomainErrors RentalSaveFailed =
      new(
         ErrorCode.BadReqest,
         Title: "Rental Persistence Failed",
         Message: "The Pick-Up Could Not Be Persisted."
      );

   public static readonly DomainErrors InvalidReservationId =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Reservation Id",
         Message: "The Provided Reservation Id Is Invalid."
      );
   


   public static readonly DomainErrors ReservationNotConfirmed =
      new(
         ErrorCode.Conflict,
         Title: "Reservation Not Confirmed",
         Message: "The Reservation Must Be Confirmed Before Pick-Up."
      );

   public static readonly DomainErrors NoCarAvailable =
      new(
         ErrorCode.Conflict,
         Title: "No Car Available",
         Message: "No Car Is Available For The Selected Category And Period."
      );
   
}