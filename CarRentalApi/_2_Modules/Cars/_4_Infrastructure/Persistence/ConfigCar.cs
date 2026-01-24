using CarRentalApi._2_Modules.Cars._3_Domain.Aggregates;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CarRentalApi._2_Modules.Cars._4_Infrastructure.Persistence;

public sealed class ConfigCar: IEntityTypeConfiguration<Car> {

   public void Configure(EntityTypeBuilder<Car> b) {
      // Table
      b.ToTable("Car");

      // PK = FK to Vehicles.Id
      b.HasBaseType<Vehicle>(); 
      
      // Properties
      b.Property(x => x.Category).HasConversion<int>().IsRequired();
      b.Property(x => x.Status).HasConversion<int>().IsRequired();
      b.Ignore(x => x.IsInMaintenance);
      b.Ignore(x => x.IsRetired);
      
      // Index: queries "available cars by category"
      b.HasIndex(x => new { x.Category, x.Status });

   }
}