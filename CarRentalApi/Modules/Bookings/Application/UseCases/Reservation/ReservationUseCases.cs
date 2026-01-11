using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
namespace CarRentalApi.Modules.Bookings.Application.UseCases;

// facade for Reservation use-cases no async or await, just pass-through
public sealed class ReservationUseCases(
   ReservationUcCreate createUc,
   ReservationUcChangePeriod changePeriodUc,
   ReservationUcConfirm confirmUc,
   ReservationUcCancel cancelUc,
   ReservationUcExpire expireUc
) : IReservationUseCases {

   public Task<Result<Guid>> CreateAsync(
      Guid customerId,
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      string? id,
      CancellationToken ct
   ) => createUc.ExecuteAsync(customerId, category, start, end, id, ct);

   public Task<Result> ChangePeriodAsync(
      Guid reservationId,
      DateTimeOffset start,
      DateTimeOffset end,
      CancellationToken ct
   ) => changePeriodUc.ExecuteAsync(reservationId, start, end, ct);

   public Task<Result> ConfirmAsync(
      Guid reservationId,
      CancellationToken ct
   ) => confirmUc.ExecuteAsync(reservationId, ct);

   public Task<Result> CancelAsync(
      Guid reservationId,
      CancellationToken ct
   ) =>  cancelUc.ExecuteAsync(reservationId, ct);
   
   public Task<Result<int>> ExpireAsync(
      CancellationToken ct
   ) => expireUc.ExecuteAsync(ct);
}
