using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Domain;

public interface ICustomerRepository {
   
   Task<Customer?> FindByIdAsync(
      Guid id, 
      CancellationToken ct
   );
   
   void Add(Customer customer);
}