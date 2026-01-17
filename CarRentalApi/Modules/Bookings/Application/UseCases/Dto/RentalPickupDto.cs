using CarRentalApi.Modules.Bookings.Domain.Enums;
namespace CarRentalApi.Modules.Bookings.Application.UseCases.Dto;

public sealed record RentalPickupDto(
   Guid ReservationId,
   int KmOut,
   RentalFuelLevel FuelOut,
   DateTimeOffset PickedupAt = default
);



