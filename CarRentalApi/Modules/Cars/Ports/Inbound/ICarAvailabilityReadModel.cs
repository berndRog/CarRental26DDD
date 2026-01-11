using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
namespace CarRentalApi.Modules.Cars.Ports.Inbound;
/// <summary>
/// Read model / policy to check if a car has overlaps with rentals or reservations
/// for a given period. Used inside the Car aggregate.
///
/// In our domain:
/// - Reservations are by CarCategory (no CarId)
/// - therefore car-specific overlap means: overlaps with Rentals
///   where Rental.ReservationId -> Reservation defines the period.
/// </summary>
public interface ICarAvailabilityReadModel {
   /// <summary>
   /// Returns true if there is any overlapping rental (car-specific) for the car.
   /// </summary>
   Task<bool> HasOverlapAsync(
      Guid carId,
      RentalPeriod period,
      CancellationToken ct
   );
}