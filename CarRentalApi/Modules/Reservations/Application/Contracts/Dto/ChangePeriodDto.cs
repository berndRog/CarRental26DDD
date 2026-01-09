namespace CarRentalApi.Modules.Reservations.Application.Contracts.Dto;

using CarRentalApi.BuildingBlocks.Enums;

public sealed record ChangePeriodDto(
   DateTimeOffset Start,
   DateTimeOffset End
);
