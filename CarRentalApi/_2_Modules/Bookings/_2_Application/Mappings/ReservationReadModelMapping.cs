using CarRentalApi._2_Modules.Bookings._2_Application.Dtos.ReadModels;
using CarRentalApi._2_Modules.Bookings._3_Domain.Aggregates;
namespace CarRentalApi._2_Modules.Bookings._2_Application.Mappings;

public static class ReservationReadModelMapping {

   public static ReservationDetailsDto ToReservationDetailsDto(this Reservation reservation) => new(
      ReservationId: reservation.Id,
      CustomerId: reservation.CustomerId,
      CarCategory: reservation.CarCategory,
      Start: reservation.Period.Start,
      End: reservation.Period.End,
      ReservationStatus: reservation.Status,
      CreatedAt: reservation.CreatedAt,
      ConfirmedAt: reservation.ConfirmedAt,
      CancelledAt: reservation.CancelledAt,
      ExpiredAt: reservation.ExpiredAt
   );

   public static ReservationListItemDto ToReservationListItemDto(this Reservation reservation) => new(
      ReservationId: reservation.Id,
      CarCategory: reservation.CarCategory,
      Start: reservation.Period.Start,
      End: reservation.Period.End,
      ReservationStatus: reservation.Status
   );
}