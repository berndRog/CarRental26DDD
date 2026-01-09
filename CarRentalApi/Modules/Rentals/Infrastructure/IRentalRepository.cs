using CarRentalApi.Modules.Rentals.Domain.Aggregates;
namespace CarRentalApi.Modules.Rentals.Infrastructure;

public interface IRentalRepository {
   // Queries (0..1)
   Task<Rental?> FindByIdAsync(
      Guid id, 
      CancellationToken ct
   );
   
   Task<Rental?> FindByReservationIdAsync(
      Guid reservationId, 
      CancellationToken ct
   ); 
   
   // Queries (0..n)
   Task<IReadOnlyList<Rental>> SelectByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct
   );
   Task<IReadOnlyList<Rental>> SelectByCarIdAsync(
      Guid carId,
      CancellationToken ct
   );
   
   // Commands
   // are sync (track entity in DbContext)
   void Add(Rental rental);
}