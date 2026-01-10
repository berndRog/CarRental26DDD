namespace CarRentalApi.Modules.Customers.Application.ReadModel.Dto;

/// <summary>
/// Filter options for customer search endpoints.
///
/// Purpose:
/// - Used by controllers to express UI-driven query criteria
/// - Can be combined freely (AND semantics)
/// - Must be translated into database queries (EF Core)
///
/// Important:
/// - This is a read-model filter, not a domain object
/// - No business rules or invariants are enforced here
///
/// Typical usage:
/// - SearchText: free search across name + email
/// - Email: exact lookup or partial match (implementation-defined)
/// - FirstName/LastName: case-insensitive partial match
/// - IsBlocked: derived state based on BlockedAt != null
/// </summary>
public sealed record CustomerSearchFilter(
   /// <summary>
   /// Free-text search across commonly relevant fields.
   ///
   /// Business meaning:
   /// - Used by search boxes in list views
   /// - Typical implementation matches:
   ///   - FirstName
   ///   - LastName
   ///   - Email
   ///
   /// Notes:
   /// - Case-insensitive match is recommended
   /// - Implementation may use "Contains" for usability
   /// </summary>
   string? SearchText = null,

   /// <summary>
   /// Email filter.
   ///
   /// Business meaning:
   /// - Used for direct lookup or narrowing down results
   ///
   /// Notes:
   /// - Prefer case-insensitive comparison
   /// - Implementation may normalize (trim/lowercase)
   /// </summary>
   string? Email = null,

   /// <summary>
   /// First name filter.
   ///
   /// Business meaning:
   /// - Used to narrow down results by first name
   ///
   /// Notes:
   /// - Case-insensitive partial match recommended
   /// </summary>
   string? FirstName = null,

   /// <summary>
   /// Last name filter.
   ///
   /// Business meaning:
   /// - Used to narrow down results by last name
   ///
   /// Notes:
   /// - Case-insensitive partial match recommended
   /// </summary>
   string? LastName = null,

   /// <summary>
   /// Blocked flag filter.
   ///
   /// Business meaning:
   /// - Used by admin/backoffice to show blocked vs active customers
   ///
   /// Technical notes:
   /// - Must be translated in queries using:
   ///   - IsBlocked == true  -> BlockedAt != null
   ///   - IsBlocked == false -> BlockedAt == null
   /// </summary>
   bool? IsBlocked = null
);
