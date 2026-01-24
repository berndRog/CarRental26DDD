using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Domain.ValueObjects;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRentalApi._2_Modules.Customers._4_Infrastructure.Persistence;

public sealed class ConfigCustomer(
   DateTimeOffsetToIsoStringConverter dtOffToIsoStrConv,
   NullableDateTimeOffsetToIsoStringConverter nulDtOffToIsoStrConv
) : IEntityTypeConfiguration<Customer> {
   public void Configure(EntityTypeBuilder<Customer> b) {
      
      b.ToTable("Customer");

      // Primary Key
      b.HasKey(x => x.Id);
      b.Property(x => x.Id).ValueGeneratedNever();

      // Scalar properties
      b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
      b.Property(x => x.LastName).HasMaxLength(100).IsRequired();

      b.Property(x => x.Email)
         .HasConversion(e => e.Value, v => Email.Create(v).Value)
         .HasMaxLength(200).IsRequired();

      // Subject (OIDC "sub") MUST exist (no legacy DB -> required)
      b.Property(x => x.Subject)
         .HasConversion(s => s.Value, v => IdentitySubject.Create(v).Value)
         .HasMaxLength(200).IsRequired();
      // Unique constraint: one subject -> one customer
      b.HasIndex(x => x.Subject).IsUnique();

      b.Property(x => x.CreatedAt)
         .HasConversion(dtOffToIsoStrConv).IsRequired();
      b.Property(x => x.BlockedAt)
         .HasConversion(nulDtOffToIsoStrConv).IsRequired(false);

      // Owned: Address (OPTIONAL)
      b.OwnsOne(c => c.Address, a => {
         // OPTIONAL: keeps columns readable & avoids collisions
         a.Property(p => p.Street)
            .HasColumnName("Address_Street").HasMaxLength(100).IsRequired(false);
         a.Property(p => p.PostalCode)
            .HasColumnName("Address_PostalCode").HasMaxLength(20).IsRequired(false);
         a.Property(p => p.City)
            .HasColumnName("Address_City").HasMaxLength(50).IsRequired(false);
         a.Property(p => p.City)
            .HasColumnName("Address_Country").HasMaxLength(30).IsRequired(false);
      });
      b.Navigation(x => x.Address).IsRequired(false);
   }
}