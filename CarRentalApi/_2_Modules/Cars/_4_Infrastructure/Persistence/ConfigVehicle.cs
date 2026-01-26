using CarRentalApi._2_Modules.Cars._3_Domain.Aggregates;
using CarRentalApi._3_Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CarRentalApi._2_Modules.Cars._2_Infrastructure.Persistence.Configurations;

public sealed class ConfigVehicle(
   DateTimeOffsetToIsoStringConverter _dtOffToIsoStrConv,
   NullableDateTimeOffsetToIsoStringConverter _nulDtOffToIsoStrConv
) : IEntityTypeConfiguration<Vehicle> {
   public void Configure(EntityTypeBuilder<Vehicle> b) {
      
      // TPT base table
      b.ToTable("Vehicles");

      // Primary Key
      b.HasKey(x => x.Id);
      b.Property(x => x.Id).ValueGeneratedNever();
      
      // Properties
      b.Property(v => v.Manufacturer).HasMaxLength(100).IsRequired();
      b.Property(v => v.Model).HasMaxLength(100).IsRequired();
      b.Property(v => v.LicensePlate).HasMaxLength(32).IsRequired();
      b.HasIndex(v => v.LicensePlate).IsUnique();
      
      b.Property(x => x.CreatedAt).HasConversion(_dtOffToIsoStrConv).IsRequired();
      b.Property(x => x.RetiredAt).HasConversion(_nulDtOffToIsoStrConv).IsRequired(false);

      // Optional: if you want optimistic concurrency (if your Entity base has it)
      // b.Property(v => v.RowVersion).IsRowVersion();
   }
}