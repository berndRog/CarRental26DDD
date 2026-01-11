using CarRentalApi.Modules.Customers.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CarRentalApi.Data.Database;

public sealed class ConfigCustomers: IEntityTypeConfiguration<Customer> {
   
   public void Configure(EntityTypeBuilder<Customer> b) {
      
      // Tablename
      b.ToTable("Customers");
     
      // Primary Key is inherited from Person
      
      // Properties
      b.Property(x => x.Id).ValueGeneratedNever();
      
      // NO relationship configuration here!
      
   }
}