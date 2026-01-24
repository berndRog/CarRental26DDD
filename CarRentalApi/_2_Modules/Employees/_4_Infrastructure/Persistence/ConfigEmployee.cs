using CarRentalApi._2_Modules.Employees._3_Domain.Aggregates;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// falls Email/Address/Phone hier liegen

namespace CarRentalApi._2_Modules.Employees._4_Infrastructure.Persistence;

public sealed class ConfigEmployee(
   DateTimeOffsetToIsoStringConverter _dtOffToIsoStrConv,
   NullableDateTimeOffsetToIsoStringConverter _nulDtOffToIsoStrConv
) : IEntityTypeConfiguration<Employee> {

   public void Configure(EntityTypeBuilder<Employee> b) {

      b.ToTable("Employee");
      
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
         .HasMaxLength(200)
         .IsRequired();
      
      // Owned: Phone (nullable)
      // (stored in Employees table)
      b.OwnsOne(x => x.Phone, pb => {
         pb.WithOwner();
         pb.Property(p => p.Number)
            .HasColumnName("PhoneNumber")
            .HasMaxLength(32)
            .IsRequired(false);
         pb.Property(p => p.Normalized)
            .HasColumnName("PhoneNormalized")
            .HasMaxLength(32)
            .IsRequired(false);
      });

      b.Navigation(x => x.Phone).IsRequired(false);
      
      // Scalar properties (Employee-specific)
      b.Property(x => x.PersonnelNumber).HasMaxLength(32).IsRequired();
      b.HasIndex(x => x.PersonnelNumber).IsUnique();

      // AdminRights enum -> int (SQLite friendly)
      b.Property(x => x.AdminRights).HasConversion<int>().IsRequired();
      // IsAdmin is computed => not persisted
      b.Ignore(x => x.IsAdmin);

      b.Property(x => x.IsActive).IsRequired();
      b.Property(x => x.CreatedAt).HasConversion(_dtOffToIsoStrConv).IsRequired();
      b.Property(x => x.DeactivatedAt).HasConversion(_nulDtOffToIsoStrConv).IsRequired(false);



      // Helpful index for "active employees"
      b.HasIndex(x => x.DeactivatedAt);
   }
}