using CarRentalApi.BuildingBlocks;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;
using CarRentalApi.Modules.Rentals.Application.Contracts;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Rentals.Application.Services;

/// <summary>
/// EF Core based implementation of <see cref="IRentalsReadApi"/>.
/// Read-only query service: no tracking, no domain mutations.
/// </summary>
public sealed class RentalsReadService(
   CarRentalDbContext _dbContext
) : IRentalsReadApi {

   public async Task<Result<Guid?>> FindRentalIdByReservationIdAsync(
      Guid reservationId,
      CancellationToken ct
   ) {
      if (reservationId == Guid.Empty) {
         return Result<Guid?>.Failure(RentalReadErrors.InvalidReservationId);
      }

      // Read-only query: AsNoTracking to avoid change tracking overhead.
      // We only need the Rental ReservationId (projection).
      var rentalId = await _dbContext.Rentals
         .AsNoTracking()
         .Where(r => r.ReservationId == reservationId)
         .Select(r => (Guid?)r.Id)
         .SingleOrDefaultAsync(ct);

      // 0 Treffer => Success(null)
      return Result<Guid?>.Success(rentalId);
   }
}
