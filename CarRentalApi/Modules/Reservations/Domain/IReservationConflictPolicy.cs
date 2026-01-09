using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Reservations.Domain.Enums;
using CarRentalApi.Modules.Reservations.Domain.ValueObjects;
namespace CarRentalApi.Modules.Reservations.Domain;

public interface IReservationConflictPolicy {
   Task<ReservationConflict> CheckAsync(
      CarCategory carCategory,
      RentalPeriod period,
      Guid ignoreReservationId,
      CancellationToken ct
   );
}
