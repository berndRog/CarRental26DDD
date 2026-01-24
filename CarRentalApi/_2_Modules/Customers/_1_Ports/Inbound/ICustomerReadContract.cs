using CarRentalApi._2_Modules.Customers._2_Application.Dtos.Contracts;
namespace CarRentalApi._2_Modules.Customers._1_Ports.Inbound;

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
public interface ICustomerReadContract {

   /// <summary>
   /// Finds a single customer by its unique identifier.
   ///
   /// Business meaning:
   /// - Used when a specific customer must be displayed or referenced
   /// - Does NOT load the aggregate for modification
   ///
   /// Returns:
   /// - Success with <see cref="CustomerContractDto"/> if the customer exists
   /// - Failure if the customer does not exist
   /// </summary>
   Task<CustomerContractDto?> FindByIdAsync(
      Guid customerId,
      CancellationToken ct
   );

   Task<CustomerContractDto?> FindByEmailAsync(
      string email, 
      CancellationToken ct
   );
   
   Task<IReadOnlyList<CustomerContractDto>> SelectByNameAsync(
      string firstName, 
      string lastName, 
      CancellationToken ct
   );
}
