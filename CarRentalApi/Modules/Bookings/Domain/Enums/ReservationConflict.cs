namespace CarRentalApi.Modules.Bookings.Domain.Enums;

public enum ReservationConflict {
   None = 0,
   NoCategoryCapacity = 1,   // there are 0 cars in the category
   OverCapacity = 2          // there are cars in the category, but all are booked in the requested period
}
