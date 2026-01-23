using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Domain.ValueObjects;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CarRentalApi._2_Modules.Customers._4_Infrastructure.Persistence;

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
      // Scalar properties
      b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
      b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
      b.Property(x => x.Email)
         .HasConversion(
            e => e.Value,
            v => Email.Create(v).Value
         )
         .HasMaxLength(200)
         .IsRequired();
      
      b.Property(x => x.CreatedAt).HasConversion(_dtOffToIsoStrConv).IsRequired();
      b.Property(x => x.BlockedAt).HasConversion(_nulDtOffToIsoStrConv).IsRequired(false);
      b.Property(x => x.IdentitySubject).HasMaxLength(200).IsRequired(false);
      
      
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
