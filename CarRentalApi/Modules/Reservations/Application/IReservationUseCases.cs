using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
namespace CarRentalApi.Modules.Reservations;

public interface IReservationUseCases {
   
   Task<Result<Reservation>> CreateAsync(
      Guid customerId,
      CarCategory carCategory,
      DateTimeOffset start,
      DateTimeOffset end,
      string? id = null,
      CancellationToken ct = default!
   );
   
   Task<Result> ChangePeriodAsync(
      Guid reservationId, 
      DateTimeOffset newStart, 
      DateTimeOffset newEnd, 
      CancellationToken ct
   );
   
   Task<Result> ConfirmAsync(
      Guid reservationId,
      CancellationToken ct = default
   );
   
   Task<Result> CancelAsync(
      Guid reservationId,
      CancellationToken ct = default
   );
   
   Task<Result<int>> ExpireAsync(
      CancellationToken ct = default
   );

}