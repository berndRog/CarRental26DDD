using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Modules.Employees.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CarRentalApi.Persistence.Database;

public class ConfigEmployees : IEntityTypeConfiguration<Employee> {
   public void Configure(EntityTypeBuilder<Employee> b) {
      // Tablename
      b.ToTable("Employees");
      
      // Primary Key is inherited from Person

      // Properties
      b.Property(e => e.PersonnelNumber)
         .IsRequired()
         .HasMaxLength(16);
      
      b.Property(a => a.AdminRights)
         .IsRequired();
   }
}
