using CarRentalApi.Modules.Bookings.Domain.Enums;
namespace CarRentalApi.Modules.Bookings.Application.UseCases.Dto;

public sealed record RentalReturnDto (
   int KmIn,
   RentalFuelLevel FuelIn,
   DateTimeOffset ReturnedAt
);



