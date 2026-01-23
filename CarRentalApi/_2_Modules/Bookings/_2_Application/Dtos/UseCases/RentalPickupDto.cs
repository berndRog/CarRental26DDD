using CarRentalApi._2_Modules.Bookings._3_Domain.Enums;
namespace CarRentalApi._2_Modules.Bookings._2_Application.Dtos.UseCases;

public sealed record RentalPickupDto(
   Guid ReservationId,
   int KmOut,
   RentalFuelLevel FuelOut,
   DateTimeOffset PickedupAt = default
);



