using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Infrastructure;
using CarRentalApi.Modules.Reservations.Application;
using CarRentalApi.Modules.Reservations.Domain.Enums;
using CarRentalApi.Modules.Reservations.Domain.ValueObjects;
using CarRentalApi.Modules.Reservations.Infrastructure;
namespace CarRentalApi.Modules.Reservations.Domain.Policies;

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

