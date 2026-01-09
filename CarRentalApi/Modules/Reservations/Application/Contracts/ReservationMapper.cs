using CarRentalApi.Modules.Reservations.Application.Contracts.Dto;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
namespace CarRentalApi.Modules.Reservations.Application.Contracts;

public static class ReservationMapper {
   
   public static ConfirmedReservationDto ToDto(
      this Reservation reservation
   ) =>
      new(
         reservation.Id,
         reservation.CustomerId,
         reservation.CarCategory,
         reservation.Period.Start,
         reservation.Period.End
      );
   
}