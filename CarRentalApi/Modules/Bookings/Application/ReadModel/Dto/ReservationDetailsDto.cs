using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Bookings.Domain.Enums;
namespace CarRentalApi.Modules.Bookings.Application.ReadModel.Dto;

public sealed record ReservationDetailsDto(
   Guid ReservationId,
   Guid CustomerId,
   CarCategory CarCategory,
   DateTimeOffset Start,
   DateTimeOffset End,
   ReservationStatus ReservationStatus,
   DateTimeOffset CreatedAt,
   DateTimeOffset? ConfirmedAt,
   DateTimeOffset? CancelledAt,
   DateTimeOffset? ExpiredAt
);
