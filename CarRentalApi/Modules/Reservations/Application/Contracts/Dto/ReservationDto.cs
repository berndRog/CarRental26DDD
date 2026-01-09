using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Reservations.Domain.Enums;

namespace CarRentalApi.Modules.Reservations.Api.Dto;

public sealed record ReservationDto(
   Guid Id,
   Guid CustomerId,
   CarCategory Category,
   DateTimeOffset Start,
   DateTimeOffset End,
   ReservationStatus Status,

   DateTimeOffset CreatedAt,
   DateTimeOffset? ConfirmedAt,
   DateTimeOffset? CancelledAt,
   DateTimeOffset? ExpiredAt,
   Guid? RentalId
);
