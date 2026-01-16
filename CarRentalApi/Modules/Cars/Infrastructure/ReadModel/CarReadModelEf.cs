using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Application.Pricing.Dto;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
using CarRentalApi.Modules.Cars.Application.Contracts.Dto;
using CarRentalApi.Modules.Cars.Application.Dto;
using CarRentalApi.Modules.Cars.Application.ReadModel;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using CarRentalApi.Modules.Cars.Application.ReadModel.Errors;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Enums;
using CarRentalApi.Modules.Cars.Ports.Inbound;
using CarRentalApi.Modules.Customers.Application.Contracts.Mapping;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Cars.Infrastructure.ReadModel;

/// <summary>
/// EF Core based read model for Cars.
/// Uses projections and AsNoTracking for optimal query performance.
/// </summary>
public sealed class CarReadModelEf(
   CarRentalDbContext _dbContext,
   IPricingPolicyCarCategories _pricing,
   IClock _clock
) : ICarReadModel {

   public async Task<Result<CarDto>> FindByIdAsync(
      Guid carId,
      CancellationToken ct
   ) {
      var car = await _dbContext.Cars
         .AsNoTracking()
         .FirstOrDefaultAsync(c => c.Id == carId, ct);

      if (car is null)
         return Result<CarDto>.Failure(DomainErrors.NotFound);

      return Result<CarDto>.Success(car.ToCarDto());
   }

   public async Task<Result<PagedResult<CarListItemDto>>> SearchAsync(
      CarSearchFilter filter,
      PageRequest page,
      SortRequest sort,
      CancellationToken ct
   ) {
      // ---------- Validation ----------
      if (page.PageNumber < 1 || page.PageSize < 1)
         return Result<PagedResult<CarListItemDto>>.Failure(DomainErrors.Invalid);

      page = page.Normalize();

      var query = _dbContext.Cars
         .AsNoTracking()
         .AsQueryable();

      // ---------- Filter ----------
      query = ApplyFilter(query, filter);

      // Count BEFORE paging
      var totalCount = await query.CountAsync(ct);

      // ---------- Sort ----------
      query = ApplySort(query, sort);

      // ---------- Paging ----------
      query = query
         .Skip(page.Skip)
         .Take(page.PageSize);

      // ---------- Projection ----------
      var items = await query
         .Select(c => c.ToCarListItem())
         .ToListAsync(ct);

      var result = new PagedResult<CarListItemDto>(
         items,
         page.PageNumber,
         page.PageSize,
         totalCount
      );

      return Result<PagedResult<CarListItemDto>>.Success(result);
   }
   
   // ----------------- helpers -----------------
   private static IQueryable<Car> ApplyFilter(
      IQueryable<Car> query,
      CarSearchFilter filter
   ) {
      if (filter.Category is not null)
         query = query.Where(c => c.Category == filter.Category.Value);

      if (filter.IsInMaintenance is not null)
         query = query.Where(c => c.IsInMaintenance == filter.IsInMaintenance.Value);

      if (filter.IsRetired is not null)
         query = query.Where(c => c.IsRetired == filter.IsRetired.Value);

      if (!string.IsNullOrWhiteSpace(filter.SearchText)) {
         var text = filter.SearchText.Trim();
         query = query.Where(c => c.LicensePlate.Contains(text));
      }

      return query;
   }

   private static IQueryable<Car> ApplySort(
      IQueryable<Car> query,
      SortRequest sort
   ) {
      var desc = sort.Direction == SortDirection.Desc;

      return sort.SortBy switch {
         CarSortFields.Category =>
            desc
               ? query.OrderByDescending(c => c.Category).ThenByDescending(c => c.LicensePlate)
               : query.OrderBy(c => c.Category).ThenBy(c => c.LicensePlate),

         CarSortFields.Maintenance =>
            desc
               ? query.OrderByDescending(c => c.IsInMaintenance).ThenByDescending(c => c.LicensePlate)
               : query.OrderBy(c => c.IsInMaintenance).ThenBy(c => c.LicensePlate),

         CarSortFields.Retired =>
            desc
               ? query.OrderByDescending(c => c.IsRetired).ThenByDescending(c => c.LicensePlate)
               : query.OrderBy(c => c.IsRetired).ThenBy(c => c.LicensePlate),

         CarSortFields.CreatedAt =>
            desc
               ? query.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.LicensePlate)
               : query.OrderBy(c => c.CreatedAt).ThenBy(c => c.LicensePlate),

         CarSortFields.LicensePlate =>
            desc
               ? query.OrderByDescending(c => c.LicensePlate)
               : query.OrderBy(c => c.LicensePlate),

         _ =>
            query.OrderBy(c => c.LicensePlate)
      };
   }
   
   
   public async Task<Result<IReadOnlyList<AvailibilityWithPriceDto>>>
      GetAvailabilityWithPriceByCategoryAsync(
         DateTimeOffset start,
         DateTimeOffset end,
         int examplesPerCategory,
         CancellationToken ct
   ) {
      
      // 1) Validate inputs
      if (examplesPerCategory < 0)
         return Result<IReadOnlyList<AvailibilityWithPriceDto>>
            .Failure(CarsApplicationErrors.InvalidLimit);

      var periodResult = RentalPeriod.Create(start, end);
      if (periodResult.IsFailure)
         return Result<IReadOnlyList<AvailibilityWithPriceDto>>
            .Failure(periodResult.Error);
      var period = periodResult.Value;

      if (period.Start < _clock.UtcNow)
         return Result<IReadOnlyList<AvailibilityWithPriceDto>>
            .Failure(CarsApplicationErrors.StartInPast);

      // categories == null/empty -> all categories
      var categories = Enum.GetValues<CarCategory>();
      
      var availCarCategories = new List<AvailibilityWithPriceDto>();

      foreach (var category in Enum.GetValues<CarCategory>()) {
         
         // Capacity per category
         var capacity = await _dbContext.Cars
            .AsNoTracking()
            .CountAsync(
               c => c.Category == category && c.Status == CarStatus.Available,
               ct);

         // Blocked by confirmed reservations
         var blocked = await _dbContext.Reservations
            .AsNoTracking()
            .Where(r => r.CarCategory == category)
            .Where(r => r.Status == ReservationStatus.Confirmed)
            // overlap condition are 
            .Where(r => r.Period.Start < end && start < r.Period.End)
            .CountAsync(ct);

         var available = Math.Max(0, capacity - blocked);
         
         // Example cars
         var examples = (examplesPerCategory == 0)
            ? []
            : await CarAvailabilityEfQueries
               .BuildAvailableCarsQuery(_dbContext, category, period)
               .OrderBy(c => c.LicensePlate)
               .Take(examplesPerCategory)
               .Select(c => c.ToCarContractDto())
               .ToListAsync(ct);
         
         // Price calculation
         var quote = _pricing.Calculate(category, start, end);

         // Add 
         var availCarCategory = new AvailibilityWithPriceDto(
            Category: category,
            AvailableCars: available,
            Total: quote.Total,
            Days: quote.Days,
            PricePerDay: quote.PricePerDay,
            DiscountPercent: quote.DiscountPercent,
            ExampleCars: examples
         );
         availCarCategories.Add(availCarCategory);
      }

      return Result<IReadOnlyList<AvailibilityWithPriceDto>>
         .Success(availCarCategories);
   }
   
   
   public async Task<Result<IReadOnlyList<AvailibilityWithPriceDto>>>
   GetAvailabilityWithPriceByCategoryAsync2(
      DateTimeOffset start,
      DateTimeOffset end,
      int examplesPerCategory,
      CancellationToken ct
) {
   // 1) Validate inputs
   if (examplesPerCategory < 0)
      return Result<IReadOnlyList<AvailibilityWithPriceDto>>
         .Failure(CarsApplicationErrors.InvalidLimit);

   var periodResult = RentalPeriod.Create(start, end);
   if (periodResult.IsFailure)
      return Result<IReadOnlyList<AvailibilityWithPriceDto>>
         .Failure(periodResult.Error);

   var period = periodResult.Value!;
   if (period.Start < _clock.UtcNow)
      return Result<IReadOnlyList<AvailibilityWithPriceDto>>
         .Failure(CarsApplicationErrors.StartInPast);

   // 2) Categories (always all 4)
   var categories = Enum.GetValues<CarCategory>();

   // 3) Capacity per category (1 query)
   var capacityMap = await _dbContext.Cars
      .AsNoTracking()
      .Where(c => c.Status == CarStatus.Available)
      .GroupBy(c => c.Category)
      .Select(g => new { Category = g.Key, Capacity = g.Count() })
      .ToDictionaryAsync(x => x.Category, x => x.Capacity, ct);

   // 4) Blocked by confirmed reservations overlapping period (1 query)
   var blockedMap = await _dbContext.Reservations
      .AsNoTracking()
      .Where(r => r.Status == ReservationStatus.Confirmed)
       // overlap with interval [start,end)
      .Where(r => r.Period.Start < end && start < r.Period.End) // 
      .GroupBy(r => r.CarCategory)
      .Select(g => new { Category = g.Key, Blocked = g.Count() })
      .ToDictionaryAsync(x => x.Category, x => x.Blocked, ct);

   // 5) Compose result
   var result = new List<AvailibilityWithPriceDto>(categories.Length);

   foreach (var category in categories) {
      capacityMap.TryGetValue(category, out var capacity);
      blockedMap.TryGetValue(category, out var blocked);

      var available = Math.Max(0, capacity - blocked);

      // Examples (max 4 categories => loop is fine)
      var examples = (examplesPerCategory == 0)
         ? new List<CarContractDto>()
         : await CarAvailabilityEfQueries
            .BuildAvailableCarsQuery(_dbContext, category, period)
            .OrderBy(c => c.LicensePlate)
            .Take(examplesPerCategory)
            .Select(c => c.ToCarContractDto())
            .ToListAsync(ct);

      // Pricing (pure in-memory)
      var quote = _pricing.Calculate(category, start, end);

      result.Add(new AvailibilityWithPriceDto(
         Category: category,
         AvailableCars: available,
         Total: quote.Total,
         Days: quote.Days,
         PricePerDay: quote.PricePerDay,
         DiscountPercent: quote.DiscountPercent,
         ExampleCars: examples
      ));
   }

   // Optional: sort by category for stable UI
   result.Sort((a, b) => a.Category.CompareTo(b.Category));

   return Result<IReadOnlyList<AvailibilityWithPriceDto>>.Success(result);
}

}