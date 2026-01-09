using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CarRentalApi.Persistence.Database;

public sealed class ConfigPeople : IEntityTypeConfiguration<Person> {
   public void Configure(EntityTypeBuilder<Person> b) {
      // Table für Basistyp
      b.ToTable("People");

      // Primary Key auf Basistyp
      b.HasKey(x => x.Id);

      // Properties
      b.Property(x => x.Id).ValueGeneratedNever();

      // Map common properties on Person (adjust names to your domain model)
      b.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
      b.Property(p => p.LastName).IsRequired().HasMaxLength(100);
      b.Property(p => p.Email).IsRequired().HasMaxLength(200);

      // Address is a value object and owned type, so it applies to all derived types
      b.OwnsOne(p => p.Address, pa => {
         pa.Property(a => a.Street).HasMaxLength(200);
         pa.Property(x => x.PostalCode).HasMaxLength(20);
         pa.Property(x => x.City).HasMaxLength(100);
      });
   }
}