using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRentalApi.Modules.Bookings.Infrastructure.Persistence;

public sealed class ConfigReservation(
   DateTimeOffsetToIsoStringConverter _dtOffToIsoStrConv,
   NullableDateTimeOffsetToIsoStringConverter _nulDtOffToIsoStrConv
) : IEntityTypeConfiguration<Reservation> {

   public void Configure(EntityTypeBuilder<Reservation> b) {

      b.ToTable("Reservation");

      // Primary Key
      b.HasKey(x => x.Id);
      b.Property(x => x.Id).ValueGeneratedNever();

      // Foreign keys
      b.Property(x => x.CustomerId).IsRequired();

      // Scalar properties
      b.Property(x => x.CarCategory).HasConversion<int>().IsRequired();

      b.Property(x => x.CreatedAt).HasConversion(_dtOffToIsoStrConv).IsRequired();
      b.Property(x => x.ConfirmedAt).HasConversion(_nulDtOffToIsoStrConv); // is not required
      b.Property(x => x.CancelledAt).HasConversion(_nulDtOffToIsoStrConv); // is not required
      b.Property(x => x.ExpiredAt).HasConversion(_nulDtOffToIsoStrConv);   // is not required

      // Prefer int enum mapping for status (smaller/faster indexes than strings)
      b.Property(x => x.Status).HasConversion<int>().IsRequired();

      // Owned value object: Period
      b.OwnsOne(r => r.Period, rp => {
         rp.WithOwner();

         rp.Property(p => p.Start)
            .HasColumnName("PeriodStart")
            .HasConversion(_dtOffToIsoStrConv)
            .IsRequired();

         rp.Property(p => p.End)
            .HasColumnName("PeriodEnd")
            .HasConversion(_dtOffToIsoStrConv)
            .IsRequired();
      });

      b.Navigation(x => x.Period).IsRequired();

      // Indexes (basic)
      b.HasIndex(x => x.CustomerId);

      // Availability search / overlap queries (SE-1/SE-2)
      // Important: use column names for owned properties
      b.HasIndex("CarCategory", "Status", "PeriodStart", "PeriodEnd");

#if OOP_MODE
      b.HasOne(res => res.Customer)
         .WithMany(c => c.Reservations)
         .HasForeignKey(res => res.CustomerId)
         .IsRequired()
         .OnDelete(DeleteBehavior.Restrict);

#elif DDD_MODE
      b.HasOne<Customer>()
         .WithMany()
         .HasForeignKey(res => res.CustomerId)
         .IsRequired()
         .OnDelete(DeleteBehavior.Restrict);
#else
      #error "Define either OOP_MODE or DDD_MODE in .csproj"
#endif
   }
}
