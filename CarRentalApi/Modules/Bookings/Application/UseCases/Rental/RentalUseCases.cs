using CarRentalApi.BuildingBlocks;
using CarRentalApi.Domain;
using CarRentalApi.Domain.UseCases.Rentals;
using CarRentalApi.Modules.Rentals;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
namespace CarRentalApi.Modules.Bookings.Application.UseCases;

// facade for Rental use-cases no async or await, just pass-through
public sealed class RentalUseCases(
   RentalUcPickup pickupUc,
   RentalUcReturn returnUc
) : IRentalUseCases {
   public Task<Result<Guid>> PickupAsync(
      Guid reservationId,
      int fuelLevelOut,
      int kmOut,
      CancellationToken ct
   ) => pickupUc.ExecuteAsync(reservationId, fuelLevelOut, kmOut, ct);

   public Task<Result> ReturnAsync(
      Guid rentalId,
      int fuelLevelIn,
      int kmIn,
      CancellationToken ct
   ) => returnUc.ExecuteAsync(rentalId, fuelLevelIn, kmIn, ct);
}