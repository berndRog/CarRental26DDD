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
   public async Task<Car?> FindByIdAsync(
      Guid id, 
      CancellationToken ct
   ) {
      return await _dbContext.Cars.FirstOrDefaultAsync(x => x.Id == id, ct);
   }

   public async Task<bool> ExistsLicensePlateAsync(
      string licensePlate,
      CancellationToken ct
   ) {
      var normalized = licensePlate.Trim().ToUpperInvariant();
      return await _dbContext.Cars
         .AnyAsync(x => x.LicensePlate == normalized, ct);
   }

   public async Task<int> CountCarsInCategoryAsync(
      CarCategory category,
      CancellationToken ct
   ) => await _dbContext.Cars
      .Where(c => c.Category == category &&
         (c.Status == CarStatus.Available || c.Status == CarStatus.Rented))
      .CountAsync(ct);
     
   public async Task<IReadOnlyList<Car>> SelectByAsync(
      CarCategory? category,
      CarStatus? status,
      CancellationToken ct
   ) {
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