using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Utils;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Bookings.Infrastructure.Repositories;


// Supplies EF Core Adapter for IReservationRepository
public sealed class ReservationRepositoryEf(
   CarRentalDbContext _dbContext,
   ILogger<ReservationRepositoryEf> _logger
) : IReservationRepository {
   
   public async Task<Reservation?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ){
      var reservation = await _dbContext.Reservations
         .FirstOrDefaultAsync(r=> r.Id == id, ct);
      if (reservation is not null) return reservation;
            
      _logger.LogDebug("Reservation not found ({ReservationId})", id.To8());
      return null;
   }

   public async Task<Reservation?> FindConfirmedByIdAsync(Guid id, CancellationToken ct) {
      _logger.LogDebug("Load Confirmed Reservation by ReservationId ({ReservationId})", id.To8());
      var reservation = await _dbContext.Reservations
         .FirstOrDefaultAsync(r=> r.Id == id && r.Status == ReservationStatus.Confirmed, ct);
      if (reservation is not null) return reservation;
      
      _logger.LogDebug("Confirmed Reservation not found ({ReservationId})", id.To8());
      return null;
   }

   public async Task<int> CountConfirmedOverlappingAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      Guid ignoreReservationId,
      CancellationToken ct
   ) {
      _logger.LogDebug(
         "Counting overlapping confirmed reservations: category={Cat}, start={Start}, end={End}, ignoreId={ReservationId}",
         category, start, end, ignoreReservationId
      );
      var count = await _dbContext.Reservations
         .Where(r =>
            r.CarCategory == category &&
            r.Status == ReservationStatus.Confirmed &&
            r.Id != ignoreReservationId &&
            // Overlap check: [start, end) overlaps [r.Start, r.End)
            start < r.Period.End && r.Period.Start < end
         )
         .CountAsync(ct);

      _logger.LogDebug(
         "Found {Count} overlapping confirmed reservations for category={CarCategory}",
         count, category
      );

      return count;
   }

   public async Task<IReadOnlyList<Reservation>> SelectDraftsToExpireAsync(
      DateTimeOffset now,
      CancellationToken ct
   ) {
      _logger.LogDebug("Load Reservation by DateTime ({dtString})", now.ToDateTimeString());
      return await _dbContext.Reservations
         .Where(r =>
            r.Status == ReservationStatus.Draft &&
            r.CreatedAt <= now)
         .ToListAsync(ct);
   }
   
   
   public void Add(Reservation reservation) {
      _logger.LogDebug("Add Reservation ({ReservationId})", reservation.Id.To8());
      _dbContext.Reservations.Add(reservation);
   }
   
}