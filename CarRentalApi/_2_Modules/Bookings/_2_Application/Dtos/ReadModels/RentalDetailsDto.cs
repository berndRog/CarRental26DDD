using CarRentalApi._2_Modules.Bookings._3_Domain.Enums;
namespace CarRentalApi._2_Modules.Bookings._2_Application.Dtos.ReadModels;

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