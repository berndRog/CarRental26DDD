using CarRentalApi._2_Modules.Bookings._2_Application.Dtos.ReadModels;
using CarRentalApi._2_Modules.Bookings._3_Domain.Aggregates;
namespace CarRentalApi._2_Modules.Bookings._2_Application.Mappings;

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