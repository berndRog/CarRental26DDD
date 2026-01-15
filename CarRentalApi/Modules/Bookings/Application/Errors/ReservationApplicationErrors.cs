using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;

namespace CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;

/// <summary>
/// Read-model-level error definitions for reservation queries.
/// </summary>
public static class ReservationApplicationErrors {

   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Reservation Id",
         Message: "The provided reservation id is invalid."
      );

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Reservation Not Found",
         Message: "The requested reservation does not exist."
      );

   public static readonly DomainErrors InvalidSortField =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Sort Field",
         Message: "The provided sort field is not supported for reservations."
      );
}