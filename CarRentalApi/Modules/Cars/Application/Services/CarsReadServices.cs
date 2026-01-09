using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Cars.Application.Contracts;
using CarRentalApi.Modules.Cars.Application.Contracts.Dto;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Domain.Policies;
using CarRentalApi.Modules.Customers.Application.Contracts.Mapping;
using CarRentalApi.Modules.Reservations.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Cars.Application.Services;

/// <summary>
/// EF Core backed implementation of <see cref="ICarsReadApi"/>.
///
/// Domain meaning:
/// - Availability is car-specific and depends on overlapping ACTIVE rentals
/// - Overlap checks are delegated to <see cref="ICarAvailabilityReadModel"/>
///
/// Use-case rule (availability query):
/// - The requested period must start in the future (Start >= Now)
/// </summary>
public sealed class CarsReadService(
   CarRentalDbContext _dbContext,
   ICarAvailabilityReadModel _availability,
   IClock _clock
) : ICarsReadApi {

   public async Task<Result<CarDto?>> FindAvailableCarAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      CancellationToken ct
   ) {
      // 1) Create domain value object (validates start < end)
      var periodResult = RentalPeriod.Create(start, end);
      if (periodResult.IsFailure) {
         return Result<CarDto?>.Failure(periodResult.Error!);
      }

      var period = periodResult.Value!;

      // 2) Use-case rule: availability is only relevant for future periods
      // Normalize to UTC if your clock is UTC (recommended)
      var now = _clock.UtcNow;
      if (period.Start.ToUniversalTime() < now) {
         return Result<CarDto?>.Failure(CarsReadErrors.StartInPast);
      }

      // 3) Load candidates (keep EF query simple)
      // Add additional filters if your model supports them (e.g. Status == Available, not in maintenance)
      var candidates = await _dbContext.Cars
         .AsNoTracking()
         .Where(c => c.Category == category)
         .OrderBy(c => c.Id)
         .ToListAsync(ct);

      // 4) First match wins (assign exactly one car)
      foreach (var car in candidates) {
         var hasOverlap = await _availability.HasOverlapAsync(car.Id, period, ct);
         if (!hasOverlap) {
            return Result<CarDto?>.Success(car.ToDto());
         }
      }

      // 0 Treffer => Success(null)
      return Result<CarDto?>.Success(null);
   }

   public async Task<Result<IReadOnlyList<CarDto>>> SelectAvailableCarsAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      int limit,
      CancellationToken ct
   ) {
      if (limit <= 0) {
         return Result<IReadOnlyList<CarDto>>.Failure(CarsReadErrors.InvalidLimit);
      }

      // 1) Create domain value object (validates start < end)
      var periodResult = RentalPeriod.Create(start, end);
      if (periodResult.IsFailure) {
         return Result<IReadOnlyList<CarDto>>.Failure(periodResult.Error!);
      }

      var period = periodResult.Value!;

      // 2) Use-case rule: availability is only relevant for future periods
      var now = _clock.UtcNow;
      if (period.Start.ToUniversalTime() < now) {
         return Result<IReadOnlyList<CarDto>>.Failure(CarsReadErrors.StartInPast);
      }

      // 3) Load candidates
      var candidates = await _dbContext.Cars
         .AsNoTracking()
         .Where(c => c.Category == category)
         .OrderBy(c => c.Id)
         .ToListAsync(ct);

      // 4) Collect up to limit available cars
      var result = new List<CarDto>(capacity: Math.Min(limit, candidates.Count));

      foreach (var car in candidates) {
         if (result.Count >= limit) break;

         var hasOverlap = await _availability.HasOverlapAsync(car.Id, period, ct);
         if (!hasOverlap) {
            result.Add(car.ToDto());
         }
      }

      // 0 Treffer => Success(empty list)
      return Result<IReadOnlyList<CarDto>>.Success(result);
   }
}