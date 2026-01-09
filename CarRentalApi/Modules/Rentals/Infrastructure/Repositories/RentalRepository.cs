using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Modules.Rentals.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CarRentalApi.Infrastructure.Persistence.Repositories;

public sealed class RentalRepository(
   CarRentalDbContext _dbContext
) : IRentalRepository {
  
   public async Task<Rental?> FindByIdAsync(
      Guid id,
      CancellationToken ct
   ) => await _dbContext.Rentals
      .FirstOrDefaultAsync(r => r.Id == id, ct);

   public async Task<Rental?> FindByReservationIdAsync(
      Guid reservationId, 
      CancellationToken ct
   ) => await _dbContext.Rentals
         .FirstOrDefaultAsync(r => r.ReservationId == reservationId, ct);
   
   public async Task<IReadOnlyList<Rental>> SelectByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct
   ) => await _dbContext.Rentals
      .AsNoTracking()
      .Where(r => r.CustomerId == customerId)
      .OrderByDescending(r => r.PickupAt)
      .ToListAsync(ct);

   public async Task<IReadOnlyList<Rental>> SelectByCarIdAsync(
      Guid carId,
      CancellationToken ct
   ) => await _dbContext.Rentals
      .AsNoTracking()
      .Where(r => r.CarId == carId)
      .OrderByDescending(r => r.PickupAt)
      .ToListAsync(ct);

   // ---------- Commands ----------
   public void Add(Rental rental) =>
      _dbContext.Rentals.Add(rental);
}