namespace CarRentalApi.Modules.Bookings.Application.Contracts.Dto;

using CarRentalApi.BuildingBlocks.Enums;

public sealed record ChangePeriodDto(
   DateTimeOffset NewStart,
   DateTimeOffset NewEnd
);
