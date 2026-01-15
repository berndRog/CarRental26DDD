using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRentalApi.Modules.Cars.Infrastructure.Persistence;

public sealed class ConfigCar(
   DateTimeOffsetToIsoStringConverter _dtOffToIsoStrConv,
   NullableDateTimeOffsetToIsoStringConverter _nulDtOffToIsoStrConv
) : IEntityTypeConfiguration<Car> {

   public void Configure(EntityTypeBuilder<Car> b) {
      // Table
      b.ToTable("Car");

      // Primary Key
      b.HasKey(x => x.Id);

      // Properties
      b.Property(x => x.Id).ValueGeneratedNever();

      b.Property(x => x.Manufacturer).HasMaxLength(100).IsRequired();
      b.Property(x => x.Model).HasMaxLength(100).IsRequired();
      b.Property(x => x.LicensePlate).HasMaxLength(32).IsRequired();

      b.Property(x => x.Category).IsRequired();
      b.Property(x => x.Status).IsRequired();
      b.Property(x => x.CreatedAt).HasConversion(_dtOffToIsoStrConv).IsRequired();
      b.Property(x => x.RetiredAt).HasConversion(_nulDtOffToIsoStrConv).IsRequired(false);
      b.Ignore(x => x.IsInMaintenance);
      b.Ignore(x => x.IsRetired);
      
      
      // Index: queries "available cars by category"
      b.HasIndex(x => new { x.Category, x.Status });
      b.HasIndex(x => x.LicensePlate).IsUnique();
      b.HasIndex(x => x.RetiredAt);

   }
}