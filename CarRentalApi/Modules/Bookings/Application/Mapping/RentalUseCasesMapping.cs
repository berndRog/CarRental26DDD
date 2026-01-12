using CarRentalApi.Modules.Bookings.Application.ReadModel.Dto;
using CarRentalApi.Modules.Bookings.Application.UseCases.Dto;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Rentals.Application.ReadModel.Dto;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;

namespace CarRentalApi.Modules.Bookings.Application.ReadModel.Mapping;

public static class RentalUseCasesMapping {

   public static RentalPickupDto ToRentalCreateDto(this Rental rental) => new(
      Id: rental.Id, 
      CarId: rental.CarId,
      CustomerId: rental.CustomerId
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