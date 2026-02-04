using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
namespace CarRentalApi._2_Modules.Customers._1_Ports.Outbound;

public interface ICustomerRepository {
   
   Task<Customer?> FindByIdAsync(
      Guid id, 
      CancellationToken ct
   );
   
   Task<Customer?> FindByIdentitySubjectAsync(
      string subject, 
      bool tracking = false,
      CancellationToken ct = default
   );
   
   Task<Customer?> FindByEmailAsync( 
      string email, 
      CancellationToken ct
   );
   
   void Add(Customer customer);
}