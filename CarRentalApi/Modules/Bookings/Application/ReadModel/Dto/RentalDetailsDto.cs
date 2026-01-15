using CarRentalApi.Modules.Bookings.Domain.Enums;
namespace CarRentalApi.Modules.Rentals.Application.ReadModel.Dto;

public sealed record RentalDetailsDto(
   Guid RentalId,
   // Foreign keys / references
   Guid ReservationId,
   Guid CarId,
   Guid CustomerId,
   // Lifecycle
   RentalStatus Status,
   // Pick-up data
   DateTimeOffset PickupAt,
   RentalFuelLevel FuelOut,   
   int KmOut,          // >= 0
   // Return data (nullable while active)
   DateTimeOffset? ReturnAt,
   RentalFuelLevel? FuelIn,  
   int? KmIn           // >= KmOut
);