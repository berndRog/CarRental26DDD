using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Reservations.Domain.Errors;

/// <summary>
/// Domain-level error definitions for reservation-related
/// invariants and business rules.
/// </summary>
public static class ReservationErrors {

   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Reservation Id",
         Message: "The Provided Reservation Id Is Invalid."
      );

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Reservation Not Found",
         Message: "The Requested Reservation Does Not Exist."
      );

   public static readonly DomainErrors InvalidStatusTransition =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Reservation Status Transition",
         Message: "The Requested Reservation Status Transition Is Not Allowed."
      );

   public static readonly DomainErrors Conflict =
      new(
         ErrorCode.Conflict,
         Title: "Reservation Conflict",
         Message: "The Car Is Already Reserved In The Selected Period."
      );

   public static readonly DomainErrors StartDateInPast =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Reservation Start Date",
         Message: "The Reservation Start Date Must Be In The Future."
      );

   public static readonly DomainErrors InvalidTimestamp =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Timestamp",
         Message: "The Provided Timestamp Is Invalid For The Current Reservation State."
      );

   public static readonly DomainErrors InvalidPeriod =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Reservation Period",
         Message: "The Reservation Period Is Invalid: The Start Date Must Be Earlier Than The End Date."
      );

   public static readonly DomainErrors NoCarCategoryCapacity =
      new(
         ErrorCode.Conflict,
         Title: "No Car Category Capacity",
         Message: "No Cars Are Available In The Selected Category For The Given Period."
      );
}
