using CarRentalApi._2_Modules.Bookings._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Bookings._4_Infrastructure.Persistence;
using CarRentalApi._2_Modules.Cars._2_Infrastructure.Persistence.Configurations;
using CarRentalApi._2_Modules.Cars._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Cars._4_Infrastructure.Persistence;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Customers._4_Infrastructure.Persistence;
using CarRentalApi._2_Modules.Employees._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Employees._4_Infrastructure.Persistence;
using CarRentalApi._4_BuildingBlocks._3_Domain.Entities;
using CarRentalApi.Modules.Bookings.Infrastructure.Persistence;
using CarRentalApi.Persistence.Database;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Data.Database;

public sealed class CarRentalDbContext(
   DbContextOptions<CarRentalDbContext> options
) : DbContext(options) {
   
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
      
      // Entity Customer -> Table Customer
      modelBuilder.ApplyConfiguration(new ConfigCustomer(dtOffToIsoStrConv, nulDtOfConv));
      // Entity Employee -> Table Employees
      modelBuilder.ApplyConfiguration(new ConfigEmployee(dtOffToIsoStrConv, nulDtOfConv));
      
      // Entity Vehicle -> Table Vehicle
      modelBuilder.ApplyConfiguration(new ConfigVehicle(dtOffToIsoStrConv, nulDtOfConv));
      // Entity Car: Vehicle -> Table Car
      modelBuilder.ApplyConfiguration(new ConfigCar());
      // Entity Reservation -> Table Reservation
      modelBuilder.ApplyConfiguration(new ConfigReservation(dtOffToIsoStrConv, nulDtOfConv));
      // Entity Rental -> Table Rental
      modelBuilder.ApplyConfiguration(new ConfigRental(dtOffToIsoStrConv, nulDtOfConv));
   }
}