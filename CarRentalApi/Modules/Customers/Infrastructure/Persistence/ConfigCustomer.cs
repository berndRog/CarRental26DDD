using CarRentalApi.Modules.Common.Domain.ValueObjects;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CarRentalApi.Modules.Customers.Infrastructure.Persistence;

public sealed class ConfigCustomer(
   DateTimeOffsetToIsoStringConverter _dtOffToIsoStrConv,
   NullableDateTimeOffsetToIsoStringConverter _nulDtOffToIsoStrConv
) : IEntityTypeConfiguration<Customer> {

   public void Configure(EntityTypeBuilder<Customer> b) {

      b.ToTable("Customer");

      // ----------------------------
      // Primary Key
      // ----------------------------
      b.HasKey(x => x.Id);
      b.Property(x => x.Id).ValueGeneratedNever();

      // ----------------------------
      // Scalar properties
      // ----------------------------
      b.Property(x => x.CreatedAt)
         .HasConversion(_dtOffToIsoStrConv)
         .IsRequired();

      b.Property(x => x.BlockedAt)
         .HasConversion(_nulDtOffToIsoStrConv)
         .IsRequired(false);

      b.Property(x => x.Identity)
         .HasMaxLength(200)
         .IsRequired(false);

      // ----------------------------
      // Owned: Contact (REQUIRED)
      // ----------------------------
      b.OwnsOne(x => x.Contact, c => {
         c.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
         c.Property(p => p.LastName).HasMaxLength(100).IsRequired();
         c.Property(p => p.Email).HasConversion(e => e.Value, v => Email.Create(v).Value)
            .HasMaxLength(200).IsRequired();

         c.OwnsOne(p => p.Phone, p => {
            p.Property(x => x.Number).HasMaxLength(40).IsRequired();
            p.Property(x => x.Normalized).HasMaxLength(40).IsRequired();
         });

      });
      b.Navigation(x => x.Contact).IsRequired();
      
      // Owned: Credentials (OPTIONAL – Phase 1)
      b.OwnsOne(x => x.Credentials, c => {
         c.Property(p => p.PasswordHash).HasColumnType("BLOB").IsRequired();
         c.Property(p => p.PasswordSalt).HasColumnType("BLOB").IsRequired();
      });
      b.Navigation(x => x.Credentials).IsRequired(false);
      
      // Owned: Address (OPTIONAL)
      b.OwnsOne(x => x.Address, a => {
         a.Property(p => p.Street).HasMaxLength(200);
         a.Property(p => p.PostalCode).HasMaxLength(20);
         a.Property(p => p.City).HasMaxLength(100);
      });
      b.Navigation(x => x.Address).IsRequired(false);
      
      // Indexes
      b.HasIndex("Contact_Email").IsUnique();
      b.HasIndex(x => x.Identity);
   }
}
