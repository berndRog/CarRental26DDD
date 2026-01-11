using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Modules.Employees.Domain.Aggregates;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Infrastructure.Persistence;
using CarRentalApi.Modules.Cars.Infrastructure.Persistence;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Data.Database;

public sealed class CarRentalDbContext(
   DbContextOptions<CarRentalDbContext> options
) : DbContext(options) {
   
   public DbSet<Person> People => Set<Person>();
   public DbSet<Customer> Customers => Set<Customer>();
   public DbSet<Employee> Employees => Set<Employee>();
   public DbSet<Car> Cars => Set<Car>();
   public DbSet<Reservation> Reservations => Set<Reservation>();
   public DbSet<Rental> Rentals => Set<Rental>();

   protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);

      // Reuse one converter instance
      var dtOffMillis = new DateTimeOffsetToIsoStringConverter();

      // TPT: Person base + derived tables
      modelBuilder.Ignore<Entity<Guid>>();
      
      // Entity Person -> Table People
      modelBuilder.ApplyConfiguration(new ConfigPeople());
      // Entity Customer -> Table Customers
      modelBuilder.ApplyConfiguration(new ConfigCustomers());
      // Entity Employee -> Table Employees
      modelBuilder.ApplyConfiguration(new ConfigEmployees());
      
      // Entity Car -> Table Cars
      modelBuilder.ApplyConfiguration(new ConfigCars());
      // Entity Reservation -> Table Reservations
      modelBuilder.ApplyConfiguration(new ConfigReservations(dtOffMillis));
      // Entity Rental -> Table Rentals
      modelBuilder.ApplyConfiguration(new ConfigRentals(dtOffMillis));
   }
}