using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Bookings.Domain.Enums;
namespace CarRentalApi.Modules.Bookings.Application.ReadModel.Dto;

public sealed class ReservationSearchFilter {
   public Guid? CustomerId { get; init; }
   public CarCategory? CarCategory { get; init; }
   public ReservationStatus? ReservationStatus { get; init; }
   public DateTimeOffset? From { get; init; }
   public DateTimeOffset? To { get; init; }
}
