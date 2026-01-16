using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Cars.Infrastructure.ReadModel;

// Reservation
// ReservationStatus
// Rental
// RentalStatus

public sealed class CarAvailabilityQueries(
   CarRentalDbContext _dbContext
) {
   // Public queries built on the shared core building blocks
   /// <summary>
   /// Pick-up: one car (first) or up to N cars (alternatives).
   /// </summary>
   public Task<Car?> FindAvailableCarEntityAsync(CarCategory category, DateTimeOffset start, DateTimeOffset end,
      CancellationToken ct)
      => AvailableCarsQuery(category, start, end)
         .OrderBy(c => c.Id)
         .FirstOrDefaultAsync(ct);

   public Task<List<Car>> SelectAvailableCarEntitiesAsync(CarCategory category, DateTimeOffset start,
      DateTimeOffset end, int limit, CancellationToken ct)
      => AvailableCarsQuery(category, start, end)
         .OrderBy(c => c.Id)
         .Take(limit)
         .ToListAsync(ct);

   /// <summary>
   /// SE-1/SE-2: availability count per category (capacity minus overlapping confirmed reservations)
   /// + example cars (using the SAME AvailableCarsQuery as pick-up)
   /// </summary>
   public async Task<IReadOnlyList<(CarCategory Category, int AvailableCars, IReadOnlyList<Car> Examples)>>
      GetAvailabilityByCategoryAsync(
         DateTimeOffset start,
         DateTimeOffset end,
         IReadOnlyList<CarCategory>? categories,
         int examplesPerCategory,
         CancellationToken ct
      ) {
      // 1) capacity per category (how many operational cars exist)
      var capacity = await FleetCapacityByCategoryQuery(categories).ToListAsync(ct);
      var capacityMap = capacity.ToDictionary(x => x.Category, x => x.Count);

      // 2) overlapping confirmed reservations per category
      var overlaps = await OverlappingConfirmedReservationsByCategoryQuery(start, end, categories).ToListAsync(ct);
      var overlapMap = overlaps.ToDictionary(x => x.Category, x => x.Count);

      // 3) merge categories
      var cats = (categories is { Count: > 0 })
         ? categories.Distinct().ToList()
         : capacityMap.Keys.Union(overlapMap.Keys).Distinct().ToList();

      cats.Sort();

      // 4) for each category: compute available count + examples
      // NOTE: examples use AvailableCarsQuery (blocks ACTIVE rentals)
      // while count uses capacity - confirmed overlaps (blocks reservations)
      var result = new List<(CarCategory, int, IReadOnlyList<Car>)>();

      foreach (var cat in cats) {
         capacityMap.TryGetValue(cat, out var cap);
         overlapMap.TryGetValue(cat, out var blockedByConfirmedReservations);

         var availableCount = Math.Max(0, cap - blockedByConfirmedReservations);

         var examples = (examplesPerCategory <= 0)
            ? new List<Car>()
            : await AvailableCarsQuery(cat, start, end)
               .OrderBy(c => c.Id)
               .Take(examplesPerCategory)
               .ToListAsync(ct);

         result.Add((cat, availableCount, examples));
      }
      return result;
   }
   

   // Core building blocks 
   private static bool Overlaps(DateTimeOffset start, DateTimeOffset end, DateTimeOffset s2, DateTimeOffset e2)
      => s2 < end && start < e2; // only for mental model; in EF use expression inline

   /// <summary>
   /// Cars that are NOT blocked by ACTIVE rentals whose reservation overlaps [start,end).
   /// Also filters operational status (Available only).
   /// This is the shared base for:
   /// - Pick-up selection (concrete cars)
   /// - SE-2 example cars (preview list)
   /// </summary>
   private IQueryable<Car> AvailableCarsQuery(CarCategory category, DateTimeOffset start, DateTimeOffset end) {
      // Blocked car ids = active rentals with overlapping CONFIRMED reservation period
      var blockedCarIds =
         _dbContext.Rentals.AsNoTracking()
            .Where(r => r.Status == RentalStatus.Active)
            .Join(
               _dbContext.Reservations.AsNoTracking(),
               rental => rental.ReservationId,
               res => res.Id,
               (rental, res) => new { rental.CarId, Res = res }
            )
            .Where(x => x.Res.Status == ReservationStatus.Confirmed) // ✅ zwingend
            .Where(x => x.Res.Period.Start < end && start < x.Res.Period.End) // overlap [start,end)
            .Select(x => x.CarId);

      // Available cars = cars in category that are operational AND not in blocked list
      return _dbContext.Cars.AsNoTracking()
         .Where(c => c.Category == category)
         .Where(c => c.Status == CarStatus.Available) // ggf. erweitern (Maintenance/Retired raus)
         .Where(c => !blockedCarIds.Contains(c.Id));
   }

   /// <summary>
   /// Overlapping confirmed reservations per category for [start,end).
   /// Used for "capacity availability" (SE-1/SE-2 counts).
   /// </summary>
   private IQueryable<(CarCategory Category, int Count)> OverlappingConfirmedReservationsByCategoryQuery(
      DateTimeOffset start,
      DateTimeOffset end,
      IReadOnlyList<CarCategory>? categories
   ) {
      var q = _dbContext.Reservations.AsNoTracking()
         .Where(r => r.Status == ReservationStatus.Confirmed) // ✅ zwingend
         .Where(r => r.Period.Start < end && start < r.Period.End);

      if (categories is { Count: > 0 })
         q = q.Where(r => categories.Contains(r.CarCategory));

      return q
         .GroupBy(r => r.CarCategory)
         .Select(g => new ValueTuple<CarCategory, int>(g.Key, g.Count()));
   }

   /// <summary>
   /// Total operational cars per category (Fleet capacity).
   /// </summary>
   private IQueryable<(CarCategory Category, int Count)> FleetCapacityByCategoryQuery(
      IReadOnlyList<CarCategory>? categories) {
      var q = _dbContext.Cars.AsNoTracking()
         .Where(c => c.Status == CarStatus.Available); // oder: != Retired && != Maintenance — je nach Regel

      if (categories is { Count: > 0 })
         q = q.Where(c => categories.Contains(c.Category));

      return q
         .GroupBy(c => c.Category)
         .Select(g => new ValueTuple<CarCategory, int>(g.Key, g.Count()));
   }

}