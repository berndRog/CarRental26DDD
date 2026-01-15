using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Application.ReadModel;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Dto;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Mapping;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using Microsoft.EntityFrameworkCore;

namespace CarRentalApi.Modules.Bookings.Infrastructure.ReadModel;

public sealed class ReservationReadModelEf(
   CarRentalDbContext _db
) : IReservationReadModel {

   private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase) {
      "id",
      "reservationid",
      "createdat",
      "start",
      "end",
      "reservationstatus",
      "carcategory",
      "status",
      "category"
   };

   public async Task<Result<ReservationDetailsDto>> FindByIdAsync(
      Guid reservationId,
      CancellationToken ct = default
   ) {
      if (reservationId == Guid.Empty)
         return Result<ReservationDetailsDto>.Failure(ReservationApplicationErrors.InvalidId);

      var reservation = await _db.Set<Reservation>()
         .AsNoTracking()
         .SingleOrDefaultAsync(r => r.Id == reservationId, ct);

      if (reservation is null)
         return Result<ReservationDetailsDto>.Failure(ReservationApplicationErrors.NotFound);

      return Result<ReservationDetailsDto>.Success(reservation.ToReservationDetailsDto());
   }

   public async Task<Result<PagedResult<ReservationListItemDto>>> SearchAsync(
      ReservationSearchFilter filter,
      PageRequest page,
      SortRequest sort,
      CancellationToken ct = default
   ) {
      page = page.Normalize();

      var sortBy = string.IsNullOrWhiteSpace(sort.SortBy)
         ? "id"
         : sort.SortBy.Trim();

      if (!AllowedSortFields.Contains(sortBy))
         return Result<PagedResult<ReservationListItemDto>>.Failure(
            ReservationApplicationErrors.InvalidSortField
         );

      IQueryable<Reservation> query = _db.Set<Reservation>().AsNoTracking();

      if (filter.CustomerId is { } customerId)
         query = query.Where(r => r.CustomerId == customerId);

      if (filter.CarCategory is { } category)
         query = query.Where(r => r.CarCategory == category);

      if (filter.ReservationStatus is { } status)
         query = query.Where(r => r.Status == status);

      if (filter.From is { } from)
         query = query.Where(r => r.Period.End >= from);

      if (filter.To is { } to)
         query = query.Where(r => r.Period.Start <= to);

      var totalCount = await query.CountAsync(ct);

      query = ApplySorting(query, sort with { SortBy = sortBy });

      var reservations = await query
         .Skip(page.Skip)
         .Take(page.PageSize)
         .ToListAsync(ct);

      var items = reservations
         .Select(r => r.ToReservationListItemDto())
         .ToList();

      var result = new PagedResult<ReservationListItemDto>(
         Items: items,
         PageNumber: page.PageNumber,
         PageSize: page.PageSize,
         TotalCount: totalCount
      );

      return Result<PagedResult<ReservationListItemDto>>.Success(result);
   }

   private static IQueryable<Reservation> ApplySorting(
      IQueryable<Reservation> query,
      SortRequest sort
   ) {
      var desc = sort.Direction == SortDirection.Desc;

      var sortBy = sort.SortBy.ToLowerInvariant();

      if (sortBy == "reservationid") sortBy = "id";
      if (sortBy == "status") sortBy = "reservationstatus";
      if (sortBy == "category") sortBy = "carcategory";

      return sortBy switch {
         "id" => desc ? query.OrderByDescending(r => r.Id)
                      : query.OrderBy(r => r.Id),

         "createdat" => desc ? query.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
                             : query.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id),

         "start" => desc ? query.OrderByDescending(r => r.Period.Start).ThenByDescending(r => r.Id)
                         : query.OrderBy(r => r.Period.Start).ThenBy(r => r.Id),

         "end" => desc ? query.OrderByDescending(r => r.Period.End).ThenByDescending(r => r.Id)
                       : query.OrderBy(r => r.Period.End).ThenBy(r => r.Id),

         "reservationstatus" => desc ? query.OrderByDescending(r => r.Status).ThenByDescending(r => r.Id)
                                     : query.OrderBy(r => r.Status).ThenBy(r => r.Id),

         "carcategory" => desc ? query.OrderByDescending(r => r.CarCategory).ThenByDescending(r => r.Id)
                               : query.OrderBy(r => r.CarCategory).ThenBy(r => r.Id),

         _ => query.OrderBy(r => r.Id)
      };
   }
}