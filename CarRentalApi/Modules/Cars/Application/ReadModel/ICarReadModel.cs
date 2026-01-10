using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
namespace CarRentalApi.Modules.Cars.Application.ReadModel;

/// <summary>
/// Read-only query model for the Cars bounded context.
///
/// Purpose:
/// - Used by API controllers and UI-facing endpoints
/// - Provides flexible read access via filtering, paging and sorting
/// - Returns projection-based read models (DTOs / ViewModels)
/// - Does NOT expose domain aggregates
/// - Does NOT modify state
///
/// Architectural intent:
/// - Serves HTTP/API read endpoints only
/// - Optimized for query use-cases and UI needs
/// - Allows efficient database projections and indexing
///
/// Important:
/// - This is NOT a bounded-context facade
/// - Other bounded contexts must use ICarsReadApi (Contracts)
/// - This interface must not be used by domain or application services
///
/// Result policy:
/// - Success:
///   - Single-item queries return the requested projection
///   - List queries return data, including empty result sets
/// - NotFound:
///   - Returned for single-item queries when the entity does not exist
/// - Invalid:
///   - Returned when input parameters are invalid
///     (e.g. paging or sorting values out of range)
/// </summary>
public interface ICarReadModel {

   /// <summary>
   /// Returns detailed information about a single car.
   ///
   /// Business meaning:
   /// - Used by detail views (e.g. admin UI, backoffice)
   /// - Shows the current state of a car including:
   ///   - category
   ///   - license plate
   ///   - maintenance and retirement status
   ///
   /// Technical notes:
   /// - Read-only projection
   /// - No domain logic is executed
   ///
   /// Returns:
   /// - Success(CarDetails) if the car exists
   /// - NotFound if the car does not exist
   /// </summary>
   Task<Result<CarDetails>> FindByIdAsync(
      Guid carId,
      CancellationToken ct
   );

   /// <summary>
   /// Searches cars using flexible filter, paging and sorting parameters.
   ///
   /// Business meaning:
   /// - Used by list views, search screens and admin dashboards
   /// - Supports combination of multiple filter criteria
   /// - Paging and sorting are always applied
   ///
   /// Technical notes:
   /// - All list-based queries are expressed via this method
   /// - No specialized list endpoints exist (e.g. "by category", "in maintenance")
   /// - Filtering, paging and sorting are translated directly into database queries
   ///
   /// Returns:
   /// - Success(PagedResult&lt;CarListItem&gt;)
   ///   - Items may be empty if no cars match the criteria
   /// - Invalid if paging or sorting parameters are invalid
   /// </summary>
   Task<Result<PagedResult<CarListItem>>> SearchAsync(
      CarSearchFilter filter,
      PageRequest page,
      SortRequest sort,
      CancellationToken ct
   );
}