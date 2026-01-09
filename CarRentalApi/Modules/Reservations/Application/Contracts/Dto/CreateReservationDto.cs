namespace CarRentalApi.Modules.Reservations.Application.Contracts.Dto;

using CarRentalApi.BuildingBlocks.Enums;

public sealed record CreateReservationDto(
   Guid CustomerId,
   CarCategory Category,
   DateTimeOffset Start,
   DateTimeOffset End
);
