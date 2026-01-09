using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
namespace CarRentalApi.Modules.Reservations.Infrastructure;

// Port of Repository Pattern
public interface IReservationRepository {
   
   // Queries (0..1)
   Task<Reservation?> FindByIdAsync(Guid id, CancellationToken ct);
   Task<Reservation?> FindConfirmedByIdAsync(Guid id, CancellationToken ct);
   
   Task<int> CountConfirmedOverlappingAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      Guid ignoreReservationId,
      CancellationToken ct
   );
   
   // Queries (0..n)
   Task<IReadOnlyList<Reservation>> SelectDraftsToExpireAsync(
      DateTimeOffset now, 
      CancellationToken ct
   );
   
   // Commands are sync, they just change the in-memory repository state
   void Add(Reservation reservation);
}


