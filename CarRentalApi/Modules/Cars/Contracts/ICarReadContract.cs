using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Cars.Application.Contracts.Dto;
using CarRentalApi.Modules.Bookings.Domain.ValueObjects;

namespace CarRentalApi.Modules.Cars.Application.Contracts;

/// <summary>
/// Read-only facade of the Cars (Fleet Management) bounded context.
/// 
/// This API is used by other bounded contexts (e.g. Rentals)
/// to query car availability without exposing the car domain model.
/// </summary>
public interface ICarReadContract {

   /// <summary>
   /// Finds a single available car for the given category and rental period.
   /// 
   /// Business meaning:
   /// - Used during pick-up to assign exactly one car to a rental
   /// - Availability must consider:
   ///   - car category
   ///   - maintenance status
   ///   - overlapping rentals for the given rental period
   /// - The rental period must start in the future (availability is forward-looking)
   ///
   /// Returns:
   /// - Success with a suitable <see cref="CarContractDto"/> if available
   /// - Success with null if no car can be assigned
   /// - Failure if input is invalid or availability cannot be evaluated
   /// </summary>
   Task<Result<CarContractDto?>> FindAvailableCarAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      CancellationToken ct
   );

   /// <summary>
   /// Selects multiple available cars for the given category and rental period.
   /// 
   /// Business meaning:
   /// - Used for UI scenarios (e.g. showing a list of selectable cars)
   /// - Useful for employee workflows or manual car selection
   /// - The rental period must start in the future
   ///
   /// Technical notes:
   /// - The result is limited by the <paramref name="limit"/> parameter
   /// - Availability rules are the same as for <see cref="FindAvailableCarAsync"/>
   ///
   /// Returns:
   /// - Success with a list of available car candidates (may be empty)
   /// - Failure if input is invalid or the query cannot be executed
   /// </summary>
   Task<Result<IReadOnlyList<CarContractDto>>> SelectAvailableCarsAsync(
      CarCategory category,
      DateTimeOffset start,
      DateTimeOffset end,
      int limit,
      CancellationToken ct
   );
}
