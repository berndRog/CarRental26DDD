using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
namespace CarRentalApi._2_Modules.Customers._1_Ports.Outbound;

public interface ICustomerRepository {
   
   Task<Customer?> FindByIdAsync(
      Guid id, 
      CancellationToken ct
   );
   
   Task<Customer?> FindByIdentitySubjectAsync( 
      IdentitySubject subject, 
      CancellationToken ct
   );
   
   Task<Customer?> FindByEmailAsync( 
      Email email, 
      CancellationToken ct
   );
   
   void Add(Customer customer);
}