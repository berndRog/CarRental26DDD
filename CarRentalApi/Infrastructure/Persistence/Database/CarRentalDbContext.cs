using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Infrastructure.Persistence;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Infrastructure.Persistence;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Infrastructure.Persistence;
using CarRentalApi.Modules.Employees.Domain.Aggregates;
using CarRentalApi.Modules.Employees.Infrastructure.Persistence;
using CarRentalApi.Modules.People.Infrastructure.Persistence;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
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
      var dtOffToIsoStrConv = new DateTimeOffsetToIsoStringConverter();
      var nulDtOfConv = new NullableDateTimeOffsetToIsoStringConverter();
      
      // TPT: Person base + derived tables
      modelBuilder.Ignore<Entity<Guid>>();
      
      // Entity Person -> Table Person
      modelBuilder.ApplyConfiguration(new ConfigPerson());
      // Entity Customer -> Table Customer
      modelBuilder.ApplyConfiguration(new ConfigCustomer(dtOffToIsoStrConv, nulDtOfConv));
      // Entity Employee -> Table Employees
      modelBuilder.ApplyConfiguration(new ConfigEmployee(dtOffToIsoStrConv, nulDtOfConv));
      
      // Entity Car -> Table Car
      modelBuilder.ApplyConfiguration(new ConfigCar(dtOffToIsoStrConv, nulDtOfConv));
      // Entity Reservation -> Table Reservation
      modelBuilder.ApplyConfiguration(new ConfigReservation(dtOffToIsoStrConv, nulDtOfConv));
      // Entity Rental -> Table Rental
      modelBuilder.ApplyConfiguration(new ConfigRental(dtOffToIsoStrConv, nulDtOfConv));
   }
}