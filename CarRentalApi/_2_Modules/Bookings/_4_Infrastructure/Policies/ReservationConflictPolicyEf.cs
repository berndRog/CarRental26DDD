using CarRentalApi._2_Modules.Bookings._3_Domain.Enums;
using CarRentalApi._2_Modules.Bookings._3_Domain.Policies;
using CarRentalApi._2_Modules.Bookings._3_Domain.ValueObjects;
using CarRentalApi._2_Modules.Cars._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain;
namespace CarRentalApi._2_Modules.Bookings._4_Infrastructure.Policies;

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

