using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Utils;
using CarRentalApi.Data.Database;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Reservations.Application;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
using CarRentalApi.Modules.Reservations.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Reservations.Infrastructure.Repositories;


// Supplies EF Core Adapter for IReservationRepository
public sealed class ReservationRepository(
   CarRentalDbContext _dbContext,
   ILogger<ReservationRepository> _logger
) : IReservationRepository {
   
   public async Task<Reservation?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ){
      _logger.LogDebug("Load Reservation by Id ({Id})", id.To8());
      var reservation = await _dbContext.Reservations
         .FirstOrDefaultAsync(r=> r.Id == id, ct);
      if (reservation is not null) return reservation;
      
      _logger.LogDebug("Reservation not found ({Id})", id.To8());
      return null;
   }

   public async Task<Reservation?> FindConfirmedByIdAsync(Guid id, CancellationToken ct) {
      _logger.LogDebug("Load Confirmed Reservation by Id ({Id})", id.To8());
      var reservation = await _dbContext.Reservations
         .FirstOrDefaultAsync(r=> r.Id == id && r.Status == ReservationStatus.Confirmed, ct);
      if (reservation is not null) return reservation;
      
      _logger.LogDebug("Confirmed Reservation not found ({Id})", id.To8());
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
         "Counting overlapping confirmed reservations: category={Cat}, start={Start}, end={End}, ignoreId={Id}",
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
      _logger.LogDebug("Add Reservation ({Id})", reservation.Id.To8());
      _dbContext.Reservations.Add(reservation);
   }
   
}