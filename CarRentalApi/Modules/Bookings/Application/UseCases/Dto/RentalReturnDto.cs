using CarRentalApi.Modules.Bookings.Domain.Enums;
namespace CarRentalApi.Modules.Bookings.Application.UseCases.Dto;

public sealed record RentalReturnDto (
   Guid RentalId,
   int KmIn,
   RentalFuelLevel FuelIn,
   DateTimeOffset ReturnAt = default
);