using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using CarRentalApi.Modules.Customers.Application.ReadModel.Dto;

namespace CarRentalApi.Modules.Customers.Application.ReadModel;

/// <summary>
/// Read-only query model for the Customers bounded context.
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
/// - Optimized for query use-cases and UI needs (search screens, lists, admin views)
/// - Allows efficient database projections and indexing
///
/// Important:
/// - This is NOT a bounded-context facade
/// - Other bounded contexts must use ICustomersReadApi (Contracts)
/// - This interface must not be used by domain services to enforce invariants
///
/// Result policy:
/// - Success:
///   - Single-item queries return the requested projection
///   - List queries return data, including empty result sets
/// - NotFound:
///   - Returned for single-item queries when the entity does not exist
/// - Invalid:
///   - Returned when input parameters are invalid
///     (e.g. paging/sorting values out of range, invalid search parameters)
/// </summary>
public interface ICustomerReadModel {

   /// <summary>
   /// Returns detailed information about a single customer.
   ///
   /// Business meaning:
   /// - Used by detail views (e.g. backoffice, admin UI)
   /// - Shows the current customer master data (name, email, address, status)
   ///
   /// Technical notes:
   /// - Read-only projection
   /// - No domain logic is executed
   ///
   /// Returns:
   /// - Success(CustomerDetail) if the customer exists
   /// - NotFound if the customer does not exist
   /// </summary>
   Task<Result<CustomerDetail>> FindByIdAsync(
      Guid Id,
      CancellationToken ct
   );

   /*
   /// <summary>
   /// Finds a single customer by email address.
   ///
   /// Business rules:
   /// - Email addresses are assumed to be unique per customer
   /// - Comparison should be case-insensitive
   ///
   /// Typical use cases:
   /// - Login / identity lookup
   /// - Duplicate email validation
   ///
   /// Returns:
   /// - Success with <see cref="CustomerDetail"/> if found
   /// - Failure if no customer with the given email exists
   /// </summary>
   Task<Result<CustomerDetail>> FindByEmailAsync(
      string email,
      CancellationToken ct
   );

   /// <summary>
   /// Finds customers by first and last name.
   ///
   /// Business rules:
   /// - Name comparison should be case-insensitive
   /// - Multiple customers may share the same name
   ///
   /// Typical use cases:
   /// - Customer search screens
   /// - Back-office administration
   ///
   /// Returns:
   /// - Success with a list of matching customers (may be empty)
   /// </summary>
   Task<Result<IReadOnlyList<CustomerListItem>>> SelectByNameAsync(
      string firstName,
      string lastName,
      CancellationToken ct
   );

   
   /// <summary>
   /// Searches customers using flexible filter, paging and sorting parameters.
   ///
   /// Business meaning:
   /// - Used by customer list views and search screens
   /// - Supports common search scenarios:
   ///   - by name (first/last name, case-insensitive)
   ///   - by email
   ///   - by status flags (e.g. blocked/active) if applicable
   /// - Paging and sorting are always applied
   ///
   /// Technical notes:
   /// - All list-based queries are expressed via this method
   /// - Filtering, paging and sorting are translated directly into database queries
   /// - Sorting must be whitelisted by allowed sort field constants
   ///
   /// Returns:
   /// - Success(PagedResult&lt;CustomerListItem&gt;)
   ///   - Items may be empty if no customers match the criteria
   /// - Invalid if paging or sorting parameters are invalid
   /// </summary>
   Task<Result<PagedResult<CustomerListItem>>> FilterAsync(
      CustomerSearchFilter filter,
      PageRequest page,
      SortRequest sort,
      CancellationToken ct
   );
   */
}
