using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Bookings.Domain.Enums;
namespace CarRentalApi.Modules.Bookings.Application.ReadModel.Dto;

public sealed record ReservationListItemDto(
   Guid ReservationId,
   CarCategory CarCategory,
   DateTimeOffset Start,
   DateTimeOffset End,
   ReservationStatus ReservationStatus
);