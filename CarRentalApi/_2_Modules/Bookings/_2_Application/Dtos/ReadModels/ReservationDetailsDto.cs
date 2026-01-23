using CarRentalApi._2_Modules.Bookings._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
namespace CarRentalApi._2_Modules.Bookings._2_Application.Dtos.ReadModels;

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
