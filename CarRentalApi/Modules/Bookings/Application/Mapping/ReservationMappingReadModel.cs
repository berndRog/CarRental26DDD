using CarRentalApi.Modules.Bookings.Application.ReadModel.Dto;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;

namespace CarRentalApi.Modules.Bookings.Application.ReadModel.Mapping;

public static class ReservationMappingReadModel {

   public static ReservationDetailsDto ToReservationDetailsDto(this Reservation reservation) => new(
      ReservationId: reservation.Id,
      CustomerId: reservation.CustomerId,
      CarCategory: reservation.CarCategory,
      Start: reservation.Period.Start,
      End: reservation.Period.End,
      ReservationStatus: reservation.Status,
      RentalId: reservation.RentalId,

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