using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Reservations.Domain.ValueObjects;
namespace CarRentalApi.Modules.Reservations.Application.Contracts.Dto;

/// <summary>
/// Data transfer object exposed to other bounded contexts.
/// Contains only data required for cross-BC orchestration.
/// </summary>
public sealed record ConfirmedReservationDto(
   Guid ReservationId,
   Guid CustomerId,
   CarCategory Category,
   DateTimeOffset Start,
   DateTimeOffset End
);



