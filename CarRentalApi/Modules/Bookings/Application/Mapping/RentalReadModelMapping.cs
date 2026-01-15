using CarRentalApi.Modules.Bookings.Application.ReadModel.Dto;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Rentals.Application.ReadModel.Dto;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;

namespace CarRentalApi.Modules.Bookings.Application.ReadModel.Mapping;

public static class RentalReadModelMapping {

   public static RentalDetailsDto ToRentalDetailsDto(this Rental rental) => new(
      RentalId: rental.Id, 
      ReservationId: rental.ReservationId,
      CarId: rental.CarId,
      CustomerId: rental.CustomerId,
      Status: rental.Status,
      PickupAt: rental.PickupAt,
      FuelOut: rental.FuelOut,
      KmOut: rental.KmOut,
      ReturnAt: rental.ReturnAt,
      FuelIn: rental.FuelIn,
      KmIn: rental.KmIn
   );

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