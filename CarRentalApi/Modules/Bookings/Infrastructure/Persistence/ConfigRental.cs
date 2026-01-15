using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CarRentalApi.Modules.Bookings.Infrastructure.Persistence;

public sealed class ConfigRental(
   DateTimeOffsetToIsoStringConverter _dtOffToIsoStrConv,
   NullableDateTimeOffsetToIsoStringConverter _nulDtOffToIsoStrConv
) : IEntityTypeConfiguration<Rental> {
   
   public void Configure(EntityTypeBuilder<Rental> b) {
      b.ToTable("Rental");
      
      // primary key
      b.HasKey(x => x.Id);
      b.Property(x => x.Id).ValueGeneratedNever();

      // foreign keys
      b.Property(x => x.ReservationId).IsRequired();
      b.Property(x => x.CustomerId).IsRequired();
      b.Property(x => x.CarId).IsRequired();

      // RentalStatus as int
      b.Property(x => x.Status).HasConversion<int>().IsRequired();

      // Pick-up
      b.Property(x => x.PickupAt).HasConversion(_dtOffToIsoStrConv).IsRequired();
      b.Property(x => x.FuelOut).HasConversion<int>().IsRequired();
      b.Property(x => x.KmOut).IsRequired();

      // Return (nullable)
      b.Property(x => x.ReturnAt).HasConversion(_nulDtOffToIsoStrConv); // not required
      b.Property(x => x.FuelIn).HasConversion<int>();                   // not required
      b.Property(x => x.KmIn);                                          // not required

      // Helpful indexes (fast lookups)
      b.HasIndex(x => x.CarId);
      b.HasIndex(x => x.ReservationId).IsUnique();
      b.HasIndex(x => new { x.CarId, x.Status });
      b.HasIndex(x => new { x.ReservationId, x.Status });
      
 #if OOP_MODE
      // Relationship: Customer <-> Rentals (1 : 0..n)
      // --------------------------------------------------
      // WITH Navigation Properties (OOP-Style)
      b.HasOne(r => r.Customer)              // Rental has one Customer
         .WithMany(c => c.Rentals)           // Customer has many Rentals
         .HasForeignKey(r => r.CustomerId)   // FK in Rentals table
         .OnDelete(DeleteBehavior.Restrict)
         .IsRequired();

      // Relationship: Car <-> Rentals (1 : 0..n)
      // --------------------------------------------------
      b.HasOne(r => r.Car)                   // Rental has one Car
         .WithMany()                         // no Navigation Property in Car
         .HasForeignKey(r => r.CarId)        // FK in Rentals table
         .OnDelete(DeleteBehavior.Restrict)
         .IsRequired();

      // Relationship: Reservation <-> Rental (1 : 0..1)
      // --------------------------------------------------
      b.HasOne(r => r.Reservation)                 // Rental has one Reservation
         .WithOne(res => res.Rental)               // Reservation has zero or one Rental
         .HasForeignKey<Rental>(r => r.ReservationId) // FK in Rental table
         .OnDelete(DeleteBehavior.Restrict)
         .IsRequired();

#elif DDD_MODE
      // Relationship: Customer <-> Rentals (1 : 0..n)
      // --------------------------------------------------
      // WITHOUT Navigation Properties (DDD-Style)
      b.HasOne<Customer>()    // Rental has one Customer (without Property)
         .WithMany()          // Customer has many Rentals (without Collection)
         .HasForeignKey(r => r.CustomerId)
         .OnDelete(DeleteBehavior.Restrict)
         .IsRequired();
         
      // Relationship: Car <-> Rentals (1 : 0..n)
      // --------------------------------------------------
      b.HasOne<Car>()         // Rental has one Car (without Property)
         .WithMany()          // Car has many Rentals (without Collection)
         .HasForeignKey(r => r.CarId)
         .OnDelete(DeleteBehavior.Restrict)
         .IsRequired();
      
      // Relationship: Reservation <-> Rental (1 : 0..1)
      // --------------------------------------------------
      b.HasOne<Reservation>() // Rental has one Reservation (without Property)
         .WithOne()           // Reservation has zero or one Rental (without Property)
         .HasForeignKey<Rental>(r => r.ReservationId)
         .OnDelete(DeleteBehavior.Restrict)
         .IsRequired();
#else
   #error "Define either OOP_MODE or DDD_MODE in .csproj"
#endif
   }
}