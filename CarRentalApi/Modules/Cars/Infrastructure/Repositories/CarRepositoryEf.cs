using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Utils;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Enums;
using CarRentalApi.Modules.Cars.Ports.Outbound;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Cars.Infrastructure.Repositories;

public sealed class CarRepositoryEf(
   CarRentalDbContext _dbContext,
   ILogger<CarRepositoryEf> _logger
) : ICarRepository {
   public async Task<Car?> FindByIdAsync(Guid id, CancellationToken ct) {
      _logger.LogDebug("Load Car by ReservationId ({ReservationId})", id.To8());
      return await _dbContext.Cars.FirstOrDefaultAsync(x => x.Id == id, ct);
   }

   public async Task<bool> ExistsLicensePlateAsync(
      string licensePlate,
      CancellationToken ct
   ) {
      var normalized = licensePlate.Trim().ToUpperInvariant();

      _logger.LogDebug("Check license plate exists ({Plate})", normalized);
      return await _dbContext.Cars
         .AnyAsync(x => x.LicensePlate == normalized, ct);
   }

   public async Task<int> CountCarsInCategoryAsync(
      CarCategory category,
      CancellationToken ct
   ) {
      _logger.LogDebug(
         "Count cars in category={CarCategory}",
         category
      );

      var count = await _dbContext.Cars
         .Where(c => c.Category == category)
         .CountAsync(ct);

      _logger.LogDebug(
         "Found {Count} cars in category={CarCategory}",
         count, category
      );

      return count;
   }

   public async Task<IReadOnlyList<Car>> SelectByAsync(
      CarCategory? category,
      CarStatus? status,
      CancellationToken ct
   ) {
      _logger.LogDebug("Select Cars category={Cat} status={CarStatus}", category, status);

      var query = _dbContext.Cars.AsQueryable();

      if (category is not null)
         query = query.Where(x => x.Category == category.Value);

      if (status is not null)
         query = query.Where(x => x.Status == status.Value);

      return await query
         .OrderBy(x => x.Category)
         .ThenBy(x => x.LicensePlate)
         .ToListAsync(ct);
   }

   public void Add(Car car) {
      _logger.LogDebug("Add Car ({ReservationId})", car.Id.To8());
      _dbContext.Cars.Add(car);
   }
}