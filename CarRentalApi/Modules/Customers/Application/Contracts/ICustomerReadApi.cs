using CarRentalApi.BuildingBlocks;
using CarRentalApi.Modules.Customers.Application.contracts.Dto;
using CarRentalApi.Modules.Customers.Application.Contracts.Dto;

namespace CarRentalApi.Modules.Customers.Application.Contracts;

/// <summary>
/// Read-only facade of the Customers bounded context.
///
/// This API exposes query operations for customer data
/// without allowing any modifications.
/// 
/// Typical usage:
/// - Controllers (HTTP GET endpoints)
/// - Other bounded contexts requiring customer information
/// - Read models / projections
/// </summary>
public interface ICustomerReadApi {

   /// <summary>
   /// Finds a single customer by its unique identifier.
   ///
   /// Business meaning:
   /// - Used when a specific customer must be displayed or referenced
   /// - Does NOT load the aggregate for modification
   ///
   /// Returns:
   /// - Success with <see cref="CustomerDto"/> if the customer exists
   /// - Failure if the customer does not exist
   /// </summary>
   Task<Result<CustomerDto>> FindByIdAsync(
      Guid customerId,
      CancellationToken ct
   );

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
   /// - Success with <see cref="CustomerDto"/> if found
   /// - Failure if no customer with the given email exists
   /// </summary>
   Task<Result<CustomerDto>> FindByEmailAsync(
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
   Task<Result<IReadOnlyList<CustomerDto>>> FindByNameAsync(
      string firstName,
      string lastName,
      CancellationToken ct
   );

   /// <summary>
   /// Executes a flexible customer search using filter criteria.
   ///
   /// Supported filter aspects may include:
   /// - Name (partial or full)
   /// - Email
   /// - Status (e.g. blocked)
   /// - Creation date ranges
   ///
   /// Business meaning:
   /// - Designed for advanced search and list views
   /// - Optimized for read-only access and pagination
   ///
   /// Returns:
   /// - Success with a list of customers matching the filter
   /// </summary>
   Task<Result<IReadOnlyList<CustomerDto>>> FilterAsync(
      CustomerFilter filter,
      CancellationToken ct
   );
}
