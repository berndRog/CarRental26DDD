using CarRentalApi._2_Modules.Bookings._3_Domain.ValueObjects;
using CarRentalApi._2_Modules.Cars._1_Ports.Inbound;
using CarRentalApi._2_Modules.Cars._2_Application.Dtos.Contracts;
using CarRentalApi._2_Modules.Cars._2_Application.Errors;
using CarRentalApi._2_Modules.Cars._2_Application.Mappings;
using CarRentalApi._2_Modules.Cars._4_Infrastructure.ReadModel;
using CarRentalApi._4_BuildingBlocks._1_Ports.Inbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi.BuildingBlocks;
using CarRentalApi.Data.Database;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi._2_Modules.Cars._4_Infrastructure.Adapters;

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
   IClock _clock
) : ICarReadContract {
   
   public async Task<Result<CarContractDto?>> FindAvailableCarAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      CancellationToken ct
   ) {
      var result = await SelectAvailableCarsAsync(category, start, end, limit: 1, ct);
      if (result.IsFailure)
         return Result<CarContractDto?>.Failure(result.Error);

      return Result<CarContractDto?>.Success(result.Value.FirstOrDefault());
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
      
      // 3) Load candidates (reuse queries)
      var carsContractDto = await CarAvailabilityEfQueries
         .BuildAvailableCarsQuery(_dbContext, category, period)
         .OrderBy(c => c.LicensePlate)
         .Select(c => c.ToCarContractDto())
         .Take(limit)
         .ToListAsync(ct);

      // 0 Treffer => Success(empty list)
      return Result<IReadOnlyList<CarContractDto>>.Success(carsContractDto);
   }
}