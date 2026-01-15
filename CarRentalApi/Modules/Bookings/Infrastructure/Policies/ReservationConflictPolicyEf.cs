using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
using CarRentalApi.Modules.Cars.Ports.Outbound;
namespace CarRentalApi.Modules.Bookings.Domain.Policies;

public sealed class ReservationConflictPolicyEf(
   IReservationRepository _reservationRepository,
   ICarRepository _carRepository
) : IReservationConflictPolicy {
   
   public async Task<ReservationConflict> CheckAsync(
      CarCategory carCategory,
      RentalPeriod period,
      Guid ignoreReservationId,
      CancellationToken ct
   ) {
      var capacity = await _carRepository.CountCarsInCategoryAsync(carCategory, ct);
      if (capacity <= 0)
         return ReservationConflict.NoCategoryCapacity;

      var overlapping = await _reservationRepository.CountConfirmedOverlappingAsync(
         carCategory,
         period.Start,
         period.End,
         ignoreReservationId,
         ct
      );

      return overlapping >= capacity
         ? ReservationConflict.OverCapacity
         : ReservationConflict.None;
   }
}

