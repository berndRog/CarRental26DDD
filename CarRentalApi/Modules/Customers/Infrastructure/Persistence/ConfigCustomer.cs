using CarRentalApi.BuildingBlocks.Domain.ValueObjects;
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
      
      // Primary Key
      b.HasKey(x => x.Id);
      b.Property(x => x.Id).ValueGeneratedNever();
      
      // Scalar properties
      b.Property(x => x.CreatedAt).HasConversion(_dtOffToIsoStrConv).IsRequired();
      b.Property(x => x.BlockedAt).HasConversion(_nulDtOffToIsoStrConv).IsRequired(false);
      b.Property(x => x.IdentitySubject).HasMaxLength(200).IsRequired(false);
      
      // Owned: Contact (REQUIRED)
      b.OwnsOne(x => x.Contact, c => {
         c.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
         c.Property(p => p.LastName).HasMaxLength(100).IsRequired();
         c.Property(p => p.Email)
            .HasConversion(e => e.Value, v => Email.Create(v).Value)
            .HasColumnName("Contact_Email")
            .HasMaxLength(200)
            .IsRequired();
         c.HasIndex(p => p.Email).IsUnique();

         c.OwnsOne(p => p.Phone, p => {
            p.Property(x => x.Number).HasMaxLength(40).IsRequired(false);
            p.Property(x => x.Normalized).HasMaxLength(40).IsRequired(false);
         });

      });
      b.Navigation(x => x.Contact).IsRequired();
      
      // Owned: Credentials (OPTIONAL – Phase 1)
      b.OwnsOne(c => c.Credentials, cc => {
         cc.Property(cr => cr.PasswordHash).HasMaxLength(512).IsRequired();
         cc.Property(cr => cr.PasswordSalt).HasMaxLength(256).IsRequired();
      });
      b.Navigation(x => x.Credentials).IsRequired(false);
      
      // Owned: Address (OPTIONAL)
      b.OwnsOne(c => c.Address, ca => {
         ca.Property(a => a.Street).HasMaxLength(200).IsRequired(false);
         ca.Property(a => a.PostalCode).HasMaxLength(20).IsRequired(false);
         ca.Property(a => a.City).HasMaxLength(100).IsRequired(false);
      });
      b.Navigation(x => x.Address).IsRequired(false);
      
      // Indexes
      b.HasIndex(x => x.IdentitySubject);

      //b.HasIndex("Contact_Email").IsUnique();

   }
}
