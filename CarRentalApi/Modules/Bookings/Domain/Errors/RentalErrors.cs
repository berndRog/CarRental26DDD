using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Rentals.Domain.Errors;

/// <summary>
/// Domain-level error definitions for rental-related validation
/// and business rule violations.
/// </summary>
public static class RentalErrors {

   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Rental ReservationId",
         Message: "The Provided Rental ReservationId Is Invalid."
      );

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Rental Not Found",
         Message: "The Requested Rental Does Not Exist."
      );

   public static readonly DomainErrors InvalidStatusTransition =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Rental ReservationStatus Transition",
         Message: "The Requested Rental ReservationStatus Transition Is Not Allowed."
      );

   public static readonly DomainErrors InvalidTimestamp =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Timestamp",
         Message: "The Provided Timestamp Is Invalid."
      );

   public static readonly DomainErrors InvalidFuelLevel =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Fuel Level",
         Message: "The Fuel Level Must Be Between 0 And 100."
      );

   public static readonly DomainErrors InvalidKm =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Kilometer Value",
         Message: "The Provided Kilometer Value Is Invalid."
      );

   public static readonly DomainErrors InvalidReservation =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Reservation Reference",
         Message: "The Reservation Reference Is Invalid."
      );

   public static readonly DomainErrors InvalidCar =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Car Reference",
         Message: "The Car Reference Is Invalid."
      );

   public static readonly DomainErrors InvalidCustomer =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Customer Reference",
         Message: "The Customer Reference Is Invalid."
      );
}
