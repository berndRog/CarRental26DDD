using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.Modules.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CarRentalApi.Modules.People.Infrastructure.Persistence;

public sealed class ConfigPerson : IEntityTypeConfiguration<Person> {

   public void Configure(EntityTypeBuilder<Person> b) {

      b.ToTable("Person");

      // Primary Key
      b.HasKey(x => x.Id);
      b.Property(x => x.Id).ValueGeneratedNever();

      // Scalar properties
      b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
      b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
      b.Property(x => x.Email)
         .HasConversion(
            e => e.Value,
            v => Email.Create(v).Value
         )
         .HasMaxLength(200).IsRequired();
      
      // Owned: Address (OPTIONAL)
      b.OwnsOne(x => x.Address, a => {
         a.Property(p => p.Street).HasMaxLength(200);
         a.Property(p => p.PostalCode).HasMaxLength(20);
         a.Property(p => p.City).HasMaxLength(100);
      });
      b.Navigation(x => x.Address).IsRequired(false);

      // Indexes
      b.HasIndex(x => x.Email).IsUnique();

      // Inheritance (TPT)
      b.UseTptMappingStrategy(); 
   }
}