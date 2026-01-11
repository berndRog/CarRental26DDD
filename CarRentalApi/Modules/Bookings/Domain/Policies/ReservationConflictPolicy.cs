using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
using CarRentalApi.Modules.Bookings.Infrastructure;
using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Infrastructure;
using CarRentalApi.Modules.Bookings.Application;
using CarRentalApi.Modules.Cars.Repositories;
namespace CarRentalApi.Modules.Bookings.Domain.Policies;

public sealed class ReservationConflictPolicy(
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

