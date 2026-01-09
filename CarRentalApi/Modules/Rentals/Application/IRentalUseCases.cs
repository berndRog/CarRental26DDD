using CarRentalApi.BuildingBlocks;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
namespace CarRentalApi.Modules.Rentals;

public interface IRentalUseCases {

   Task<Result<Rental>> PickupAsync(
      Guid reservationId,
      Guid customerId,
      Guid carId,
      int fuelLevelOut,
      int kmOut,
      CancellationToken ct
   );

   Task<Result<Rental>> ReturnAsync(
      Guid rentalId,
      int fuelLevelIn,
      int kmIn,
      CancellationToken ct
   );

}