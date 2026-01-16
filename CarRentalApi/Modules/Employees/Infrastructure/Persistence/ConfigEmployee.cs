using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.Modules.Employees.Domain.Aggregates;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// falls Email/Address/Phone hier liegen

namespace CarRentalApi.Modules.Employees.Infrastructure.Persistence;

public sealed class ConfigEmployee(
   DateTimeOffsetToIsoStringConverter _dtOffToIsoStrConv,
   NullableDateTimeOffsetToIsoStringConverter _nulDtOffToIsoStrConv
) : IEntityTypeConfiguration<Employee> {

   public void Configure(EntityTypeBuilder<Employee> b) {

      // TPT: Derived table
      // (Base mapping is in ConfigPerson)
      b.ToTable("Employee");
      b.HasBaseType<Person>();
      
      // PrimaryKey is inherited from Person (Id)
      b.Property(x => x.Id).ValueGeneratedNever();

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

      // Owned: Phone (nullable)
      // (stored in Employees table)
      b.OwnsOne(x => x.Phone, pb => {
         pb.WithOwner();
         pb.Property(p => p.Number)
            .HasColumnName("PhoneNumber").HasMaxLength(32).IsRequired(false);
         pb.Property(p => p.Normalized).HasColumnName("PhoneNormalized").HasMaxLength(32).IsRequired(false);
      });

      b.Navigation(x => x.Phone).IsRequired(false);

      // Helpful index for "active employees"
      b.HasIndex(x => x.DeactivatedAt);
   }
}