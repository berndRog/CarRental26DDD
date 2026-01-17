using CarRentalApi.Modules.Bookings.Application.UseCases.Dto;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
namespace CarRentalApi.Modules.Bookings.Application.ReadModel.Mapping;

public static class RentalUseCasesMapping {

   // public static RentalPickupDto ToRentalCreateDto(this Rental rental) => new(
   //    FuelOut: rental.FuelOut,
   //    KmOut: rental.KmOut,
   //    PickedUpAt: rental.PickupAt
   // );

   /*
   public static ReservationListItemDto ToReservationListItemDto(this Reservation reservation) => new(
      ReservationId: reservation.Id,
      CarCategory: reservation.CarCategory,
      Start: reservation.Period.Start,
      End: reservation.Period.End,
      ReservationStatus: reservation.Status
   );
   */
}