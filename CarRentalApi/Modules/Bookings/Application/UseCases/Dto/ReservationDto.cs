using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Bookings.Domain.Enums;

namespace CarRentalApi.Modules.Bookings.Api.Dto;

public sealed record ReservationDto(
   Guid CustomerId,
   CarCategory CarCategory,
   DateTimeOffset Start,
   DateTimeOffset End,
   string? Id
);
