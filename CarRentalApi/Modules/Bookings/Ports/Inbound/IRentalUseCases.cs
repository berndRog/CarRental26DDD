using CarRentalApi.BuildingBlocks;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
namespace CarRentalApi.Modules.Rentals;

public interface IRentalUseCases {

   Task<Result<Guid>> PickupAsync(
      Guid reservationId,
      int fuelLevelOut,
      int kmOut,
      CancellationToken ct
   );

   Task<Result> ReturnAsync(
      Guid rentalId,
      int fuelLevelIn,
      int kmIn,
      CancellationToken ct
   );

}