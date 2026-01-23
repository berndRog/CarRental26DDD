using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
namespace CarRentalApi._2_Modules.Bookings._2_Application.Dtos.UseCases;

public sealed record ReservationDto(
   Guid CustomerId,
   CarCategory CarCategory,
   DateTimeOffset Start,
   DateTimeOffset End,
   string? Id
);
