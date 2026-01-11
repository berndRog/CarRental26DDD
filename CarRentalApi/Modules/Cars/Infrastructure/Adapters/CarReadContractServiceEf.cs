using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
using CarRentalApi.Modules.Cars.Application.Contracts;
using CarRentalApi.Modules.Cars.Application.Contracts.Dto;
using CarRentalApi.Modules.Cars.Application.ReadModel;
using CarRentalApi.Modules.Cars.Application.ReadModel.Errors;
using CarRentalApi.Modules.Customers.Application.Contracts.Mapping;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Cars.Infrastructure.Adapters;

/// <summary>
/// EF Core backed implementation of <see cref="ICarReadContract"/>.
///
/// Domain meaning:
/// - Availability is car-specific and depends on overlapping ACTIVE rentals
/// - Overlap checks are delegated to <see cref="ICarAvailabilityReadModel"/>
///
/// Use-case rule (availability query):
/// - The requested period must start in the future (Start >= Now)
/// </summary>
public sealed class CarReadContractServiceEf(
   CarRentalDbContext _dbContext,
   ICarAvailabilityReadModel _availability,
   IClock _clock
) : ICarReadContract {

   public async Task<Result<CarContractDto?>> FindAvailableCarAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      CancellationToken ct
   ) {
      // 1) Create domain value object (validates start < end)
      var periodResult = RentalPeriod.Create(start, end);
      if (periodResult.IsFailure) 
         return Result<CarContractDto?>.Failure(periodResult.Error);
      var period = periodResult.Value!;

      // 2) Use-case rule: availability is only relevant for future periods
      // Normalize to UTC if your clock is UTC (recommended)
      var now = _clock.UtcNow;
      if (period.Start < now) 
         return Result<CarContractDto?>.Failure(CarsReadErrors.StartInPast);
      
      // 3) Load candidates (keep EF query simple)
      // Add additional filters if your model supports them (e.g. ReservationStatus == Available, not in maintenance)
      var candidates = await _dbContext.Cars
         .AsNoTracking()
         .Where(c => c.Category == category)
         .OrderBy(c => c.Id)
         .ToListAsync(ct);

      // 4) First match wins (assign exactly one car)
      foreach (var car in candidates) {
         var hasOverlap = await _availability.HasOverlapAsync(car.Id, period, ct);
         if (!hasOverlap) {
            return Result<CarContractDto?>.Success(car.ToCarContractDto());
         }
      }

      // 0 Treffer => Success(null)
      return Result<CarContractDto?>.Success(null);
   }

   public async Task<Result<IReadOnlyList<CarContractDto>>> SelectAvailableCarsAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      int limit,
      CancellationToken ct
   ) {
      if (limit <= 0) 
         return Result<IReadOnlyList<CarContractDto>>.Failure(CarsReadErrors.InvalidLimit);
      

      // 1) Create domain value object (validates start < end)
      var periodResult = RentalPeriod.Create(start, end);
      if (periodResult.IsFailure) 
         return Result<IReadOnlyList<CarContractDto>>.Failure(periodResult.Error);
      var period = periodResult.Value!;

      // 2) Use-case rule: availability is only relevant for future periods
      var now = _clock.UtcNow;
      if (period.Start < now) 
         return Result<IReadOnlyList<CarContractDto>>.Failure(CarsReadErrors.StartInPast);

      // 3) Load candidates
      var candidates = await _dbContext.Cars
         .AsNoTracking()
         .Where(c => c.Category == category)
         .OrderBy(c => c.Id)
         .ToListAsync(ct);

      // 4) Collect up to limit available cars
      var result = new List<CarContractDto>(capacity: Math.Min(limit, candidates.Count));

      foreach (var car in candidates) {
         if (result.Count >= limit) break;

         var hasOverlap = await _availability.HasOverlapAsync(car.Id, period, ct);
         if (!hasOverlap) {
            result.Add(car.ToCarContractDto());
         }
      }

      // 0 Treffer => Success(empty list)
      return Result<IReadOnlyList<CarContractDto>>.Success(result);
   }
}