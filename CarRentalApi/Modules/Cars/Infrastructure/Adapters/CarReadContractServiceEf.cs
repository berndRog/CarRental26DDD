using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
using CarRentalApi.Modules.Cars.Application.Contracts;
using CarRentalApi.Modules.Cars.Application.Contracts.Dto;
using CarRentalApi.Modules.Cars.Application.ReadModel.Errors;
using CarRentalApi.Modules.Cars.Domain.Enums;
using CarRentalApi.Modules.Cars.Ports.Inbound;
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

   public async Task<Result<IReadOnlyList<CarAvailabilityContractDto>>> GetAvailabilityByCategoryAsync(
      DateTimeOffset start,
      DateTimeOffset end,
      IReadOnlyList<CarCategory>? categories,
      int examplesPerCategory,
      CancellationToken ct
   ) {
      // Validation 
      if (start >= end)
         return Result<IReadOnlyList<CarAvailabilityContractDto>>.Failure(
            CarsApplicationErrors.InvalidPeriod
         );

      if (examplesPerCategory < 0)
         return Result<IReadOnlyList<CarAvailabilityContractDto>>.Failure(
            CarsApplicationErrors.InvalidExamplesPerCategory
         );
      
      var filterCategories = (categories is { Count: > 0 }) ? categories : null;

      // Overlap condition: [start, end)
      // overlap if existing.Start < end && existing.End > start
      
      // 1) Eligible cars (operationally usable)
      var eligibleCars = _dbContext.Cars
         .AsNoTracking()
         .Where(c =>
            c.Status == CarStatus.Available &&
            (filterCategories == null || filterCategories.Contains(c.Category))
         );

      // 2) Blocked categories by CONFIRMED reservations overlapping the period
      // IMPORTANT:
      // For SE-1/SE-2 it is enough to block by confirmed reservations.
      // Rentals can be added later once we know the correct Rental time fields.
      var blockedCategories = _dbContext.Reservations
         .AsNoTracking()
         .Where(r =>
            r.Status == ReservationStatus.Confirmed &&
            r.Period.Start < end &&
            r.Period.End > start
         )
         .Select(r => r.CarCategory);

      // 3) Available cars: eligible AND category not blocked beyond capacity
      // SIMPLE CAPACITY RULE:
      // - Count available cars per category
      // - Count overlapping confirmed reservations per category
      // - AvailableCars = carsInCategory - overlappingReservations
      //
      // We compute both counts and then compose result.
      var carsInCategory = await eligibleCars
         .GroupBy(c => c.Category)
         .Select(g => new {
            Category = g.Key,
            Cars = g.Count()
         })
         .ToListAsync(ct);

      // Overlapping confirmed reservations per category
      var overlappingReservations = await _dbContext.Reservations
         .AsNoTracking()
         .Where(r =>
            r.Status == ReservationStatus.Confirmed &&
            r.Period.Start < end &&
            r.Period.End > start
         )
         .GroupBy(r => r.CarCategory)
         .Select(g => new {
            Category = g.Key,
            Count = g.Count()
         })
         .ToListAsync(ct);

      var overlapDict = overlappingReservations.ToDictionary(x => x.Category, x => x.Count);

      // For examples we need actual cars that are available candidates.
      // For "simple" implementation we just take available cars from eligibleCars
      // and later cap examples per category. Since we have only few categories this is OK.
      var orderedCars = await eligibleCars
         .OrderBy(c => c.Manufacturer)
         .ThenBy(c => c.Model)
         .ThenBy(c => c.LicensePlate)
         .Select(c => new CarContractDto(
            c.Id,
            c.Manufacturer,
            c.Model,
            c.LicensePlate,
            c.Category,
            c.Status
         ))
         .ToListAsync(ct);

      var examplesByCategory = orderedCars
         .GroupBy(x => x.Category)
         .ToDictionary(
            g => g.Key,
            g => (IReadOnlyList<CarContractDto>)g.Take(examplesPerCategory).ToList()
         );

      var result = carsInCategory
         .Select(x => {
            var overlap = overlapDict.TryGetValue(x.Category, out var cnt) ? cnt : 0;
            var availableCount = Math.Max(0, x.Cars - overlap);

            return new CarAvailabilityContractDto(
               x.Category,
               availableCount,
               examplesByCategory.TryGetValue(x.Category, out var ex)
                  ? ex
                  : Array.Empty<CarContractDto>()
            );
         })
         .Where(x => x.AvailableCars > 0) // only show available categories
         .OrderBy(x => x.Category)
         .ToList();


      return Result<IReadOnlyList<CarAvailabilityContractDto>>.Success(result);



   }

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
         return Result<CarContractDto?>.Failure(CarsApplicationErrors.StartInPast);

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
         return Result<IReadOnlyList<CarContractDto>>.Failure(CarsApplicationErrors.InvalidLimit);

      // 1) Create domain value object (validates start < end)
      var periodResult = RentalPeriod.Create(start, end);
      if (periodResult.IsFailure)
         return Result<IReadOnlyList<CarContractDto>>.Failure(periodResult.Error);
      var period = periodResult.Value!;

      // 2) Use-case rule: availability is only relevant for future periods
      var now = _clock.UtcNow;
      if (period.Start < now)
         return Result<IReadOnlyList<CarContractDto>>.Failure(CarsApplicationErrors.StartInPast);

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