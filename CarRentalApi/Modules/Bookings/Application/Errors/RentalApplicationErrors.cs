using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;

/// <summary>
/// Use-case level error definitions for rental-related application flows
/// (e.g. pick-up, return).
/// </summary>
public static class RentalApplicationErrors {
   
   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Rental Id",
         Message: "The provided rental id is invalid."
      );
   
   public static readonly DomainErrors ReservationNotFound =
      new(
         ErrorCode.NotFound,
         Title: "Reservation Not Found",
         Message: "The Requested Reservation Does Not Exist."
      );
   
   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Rental Not Found",
         Message: "The Requested Rental Does Not Exist."
      );

   public static readonly DomainErrors ReservationInvalidStatus =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Reservation ReservationStatus",
         Message: "The Reservation ReservationStatus Does Not Allow Pick-Up."
      );
   
   public static readonly DomainErrors RentalAlreadyExistsForReservation =
      new(
         ErrorCode.Conflict,
         Title: "Rental already exists",
         Message: "A rental for the given reservation already exists."
      );

   public static readonly DomainErrors RentalSaveFailed =
      new(
         ErrorCode.BadRequest,
         Title: "Rental Persistence Failed",
         Message: "The Pick-Up Could Not Be Persisted."
      );

   public static readonly DomainErrors InvalidReservationId =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Reservation ReservationId",
         Message: "The Provided Reservation ReservationId Is Invalid."
      );
   
   public static readonly DomainErrors RentalIdMismatch =
      new(
         ErrorCode.BadRequest,
         Title: "Rental Id Mismatch",
         Message: "The rental id in the route does not match the rental id in the request body."
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
         Message: "No Car Is Available For The Selected CarCategory And Period."
      );
   
}