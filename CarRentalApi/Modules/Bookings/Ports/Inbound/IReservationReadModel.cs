using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Bookings.Api.Dto;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Dto;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;

namespace CarRentalApi.Modules.Bookings.Application.ReadModel;

/// <summary>
/// Read-only query model for the Booking bounded context (Reservations).
///
/// Purpose:
/// - Used by API controllers and UI-facing endpoints
/// - Provides read access to reservation data via projections (DTOs)
/// - Supports detail views and list/search screens
/// - Optimized for query use-cases (no tracking, no aggregates)
///
/// Architectural intent:
/// - Serves HTTP/API read endpoints only
/// - Separates read concerns from write/use-case logic
/// - Allows efficient database projections and indexing
///
/// Important:
/// - This is NOT a bounded-context facade
/// - Other bounded contexts must use IBookingsReadApi (Contracts), if exposed
/// - This interface must NOT be used by domain services or use cases
///   to enforce invariants or modify state
///
/// Result policy:
/// - Success:
///   - Single-item queries return the requested projection
///   - Search queries return paged result sets (may be empty)
/// - NotFound:
///   - Returned for single-item queries when the reservation does not exist
/// - Invalid:
///   - Returned when input parameters are invalid
///     (e.g. paging/sorting values out of range, invalid filter combinations)
/// </summary>
public interface IReservationReadModel {

   /// <summary>
   /// Returns detailed information about a single reservation.
   ///
   /// Business meaning:
   /// - Used by reservation detail views (customer or back-office)
   /// - Shows the current state of the reservation lifecycle
   ///   (e.g. Draft, Confirmed, Cancelled, Expired)
   ///
   /// Technical notes:
   /// - Read-only projection
   /// - No domain logic or state transitions are executed
   /// - Data is retrieved using no-tracking queries
   ///
   /// Returns:
   /// - Success(<see cref="ReservationDetailsDto"/>) if the reservation exists
   /// - NotFound if no reservation with the given id exists
   /// </summary>
   Task<Result<ReservationDetailsDto>> FindByIdAsync(
      Guid reservationId,
      CancellationToken ct = default
   );

   /// <summary>
   /// Searches reservations using flexible filter, paging and sorting parameters.
   ///
   /// Business meaning:
   /// - Used by reservation list and search screens
   /// - Supports common scenarios:
   ///   - "My reservations" (filter by customer)
   ///   - Back-office administration
   ///   - Filtering by status or rental period
   ///
   /// Technical notes:
   /// - Paging is controlled via <see cref="PageRequest"/>
   /// - Sorting is controlled via <see cref="SortRequest"/>
   /// - Queries are optimized for read access (no aggregates, no tracking)
   /// - Sorting must be restricted to a predefined set of allowed fields
   ///
   /// Returns:
   /// - Success(<see cref="PagedResult{ReservationListItemDto}"/>)
   ///   - Items may be empty if no reservations match the criteria
   /// - Invalid if paging or sorting parameters are invalid
   /// </summary>
   Task<Result<PagedResult<ReservationListItemDto>>> SearchAsync(
      ReservationSearchFilter filter,
      PageRequest page,
      SortRequest sort,
      CancellationToken ct = default
   );
}
