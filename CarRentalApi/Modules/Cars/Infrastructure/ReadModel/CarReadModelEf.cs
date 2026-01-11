using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Errors;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Cars.Application.ReadModel;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Application.Contracts.Mapping;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Cars.Infrastructure.ReadModel;

/// <summary>
/// EF Core based read model for Cars.
/// Uses projections and AsNoTracking for optimal query performance.
/// </summary>
public sealed class CarReadModelEf(
   CarRentalDbContext _dbContext
) : ICarReadModel {

   public async Task<Result<CarDetails>> FindByIdAsync(
      Guid carId,
      CancellationToken ct
   ) {
      var carDetails = await _dbContext.Cars
         .AsNoTracking()
         .Where(c => c.Id == carId)
         .Select(c => c.ToCarDetails())
         .SingleOrDefaultAsync(ct);

      if (carDetails is null)
         return Result<CarDetails>.Failure(DomainErrors.NotFound);

      return Result<CarDetails>.Success(carDetails);
   }

   public async Task<Result<PagedResult<CarListItem>>> SearchAsync(
      CarSearchFilter filter,
      PageRequest page,
      SortRequest sort,
      CancellationToken ct
   ) {
      // ---------- Validation ----------
      if (page.PageNumber < 1 || page.PageSize < 1)
         return Result<PagedResult<CarListItem>>.Failure(DomainErrors.Invalid);

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

      var result = new PagedResult<CarListItem>(
         items,
         page.PageNumber,
         page.PageSize,
         totalCount
      );

      return Result<PagedResult<CarListItem>>.Success(result);
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
}