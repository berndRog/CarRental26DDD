using System.Data.Common;
using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Domain.Policies;
using CarRentalApi.Modules.Cars.Infrastructure.ReadModels;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Application.ReadModel;
using CarRentalApi.Modules.Cars.Application.ReadModel.Errors;
using CarRentalApi.Modules.Cars.Infrastructure.Adapters;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CarRentalApiTest.Modules.Cars.Integration;

public sealed class CarReadService_IntT  : TestBase, IAsyncLifetime { 

   private DbConnection _dbConnection = null!; 
   private CarRentalDbContext _dbContext = null!;
   private TestSeed _seed = null!;
   
   public async Task InitializeAsync() {
      _seed = new TestSeed();
      
      _dbConnection = new SqliteConnection("Filename=:memory:");
      await _dbConnection.OpenAsync();

      var options = new DbContextOptionsBuilder<CarRentalDbContext>()
         .UseSqlite(_dbConnection)
         .EnableSensitiveDataLogging()
         .Options;

      _dbContext = new CarRentalDbContext(options);
      await _dbContext.Database.EnsureCreatedAsync();
      
      // Seed base aggregates (FK safety: customers before reservations, cars before rentals)
      _dbContext.Customers.AddRange(_seed.Customers);
      _dbContext.Cars.AddRange(_seed.Cars);
      _dbContext.Reservations.AddRange(_seed.Reservations);
      _dbContext.Rentals.AddRange(_seed.Rentals);

      await _dbContext.SaveChangesAsync();
   }

   public async Task DisposeAsync() {
      if (_dbContext != null) {
         await _dbContext.DisposeAsync();
         _dbContext = null!;
      }

      if (_dbConnection != null) {
         await _dbConnection.CloseAsync();
         await _dbConnection.DisposeAsync();
         _dbConnection = null!;
      }
   }

   private static CarReadContractServiceEf CreateSut(
      CarRentalDbContext db,
      TestSeed seed
   ) {
      ICarAvailabilityReadModel availability = new CarAvailabilityReadModelEf(db);
      IClock clock = new FakeClock(seed.FixedNow);
      return new CarReadContractServiceEf(db, availability, clock);
   }

   private static async Task ConfirmReservationsAsync(
      CarRentalDbContext dbContext, 
      IEnumerable<Reservation> reservations, 
      DateTimeOffset now
   ) {
      // Important: your ICarAvailabilityReadModel filters:
      // - rental.ReservationStatus == Active
      // - reservation.ReservationStatus == Confirmed
      foreach (var reservation in reservations) {
         var result = reservation.Confirm(now);
         Assert.True(result.IsSuccess);
      }
      await dbContext.SaveChangesAsync();
   }


   [Fact]
   public async Task FindAvailableCarAsync_StartInPast_ReturnsFailure_StartInPast() {
      
      var sut = CreateSut(_dbContext, _seed);

      var start = _seed.FixedNow.AddMinutes(-1); // start < now => must fail
      var end = _seed.FixedNow.AddHours(1);

      var result = await sut.FindAvailableCarAsync(
         category: CarCategory.Compact,
         start: start,
         end: end,
         ct: CancellationToken.None
      );

      Assert.True(result.IsFailure);
      Assert.Equal(CarsReadErrors.StartInPast.Code, result.Error!.Code);
   }

   [Fact]
   public async Task SelectAvailableCarsAsync_LimitLessOrEqualZero_ReturnsFailure_InvalidLimit() {
      
      var sut = CreateSut(_dbContext, _seed);

      var result = await sut.SelectAvailableCarsAsync(
         category: CarCategory.Compact,
         start: _seed.Period1.Start,
         end: _seed.Period1.End,
         limit: 0,
         ct: CancellationToken.None
      );

      Assert.True(result.IsFailure);
      Assert.Equal(CarsReadErrors.InvalidLimit.Code, result.Error!.Code);
   }

   [Fact]
   public async Task FindAvailableCarAsync_WhenCarsWithActiveRentalsOverlap_ReturnsNextFreeCar() {
      
      // Confirm the reservations that are referenced by active rentals in the seed,
      // otherwise the overlap policy will not see them.
      await ConfirmReservationsAsync(
         _dbContext,
         new[] { _seed.Reservation1, _seed.Reservation2, _seed.Reservation3 },
         now: _seed.FixedNow
      );

      var sut = CreateSut(_dbContext, _seed);

      // Seed rentals block Car6/Car7/Car8 for Period1/Overlap1.
      // Candidates are ordered by ReservationId => Car6, Car7, Car8, Car9, Car10 ...
      // Expected: first available car should be Car9.
      var result = await sut.FindAvailableCarAsync(
         category: CarCategory.Compact,
         start: _seed.Period1.Start,
         end: _seed.Period1.End,
         ct: CancellationToken.None
      );

      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Value);
      Assert.Equal(_seed.Car9.Id, result.Value!.Id);
   }

   [Fact]
   public async Task SelectAvailableCarsAsync_ReturnsMultipleFreeCars_RespectsLimit() {

      await ConfirmReservationsAsync(
         _dbContext,
         new[] { _seed.Reservation1, _seed.Reservation2, _seed.Reservation3 },
         now: _seed.FixedNow
      );

      var sut = CreateSut(_dbContext, _seed);

      var result = await sut.SelectAvailableCarsAsync(
         category: CarCategory.Compact,
         start: _seed.Period1.Start,
         end: _seed.Period1.End,
         limit: 2,
         ct: CancellationToken.None
      );

      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Value);
      Assert.Equal(2, result.Value!.Count);

      // Should skip Car6/7/8 (overlapped) and take Car9 + Car10
      Assert.Equal(_seed.Car9.Id, result.Value[0].Id);
      Assert.Equal(_seed.Car10.Id, result.Value[1].Id);
   }

   [Fact]
   public async Task FindAvailableCarAsync_WhenAllCarsInCategoryOverlap_ReturnsSuccess_Null() {

      // Confirm reservations used by existing seed rentals
      await ConfirmReservationsAsync(
         _dbContext,
         new[] { _seed.Reservation1, _seed.Reservation2, _seed.Reservation3 },
         now: _seed.FixedNow
      );

      // Block also Car9 and Car10 by creating additional active rentals
      // using already existing reservations (4 and 5) and confirming them.
      await ConfirmReservationsAsync(
         _dbContext,
         new[] { _seed.Reservation4, _seed.Reservation5 },
         now: _seed.FixedNow
      );

      var extraRental4 = Rental.CreateAtPickup(
         reservationId: _seed.Reservation4.Id,
         customerId: _seed.Customer2.Id,
         carId: _seed.Car9.Id,
         pickupAt: _seed.Period1.Start.AddMinutes(5),
         fuelLevelOut: 80,
         kmOut: 40_000,
         id: "40000000-0000-0000-0000-000000000000"
      );
      Assert.True(extraRental4.IsSuccess);

      var extraRental5 = Rental.CreateAtPickup(
         reservationId: _seed.Reservation5.Id,
         customerId: _seed.Customer2.Id,
         carId: _seed.Car10.Id,
         pickupAt: _seed.Period1.Start.AddMinutes(10),
         fuelLevelOut: 90,
         kmOut: 50_000,
         id: "50000000-0000-0000-0000-000000000000"
      );
      Assert.True(extraRental5.IsSuccess);

      _dbContext.Rentals.AddRange(extraRental4.Value!, extraRental5.Value!);
      await _dbContext.SaveChangesAsync();

      var sut = CreateSut(_dbContext, _seed);

      var result = await sut.FindAvailableCarAsync(
         category: CarCategory.Compact,
         start: _seed.Period1.Start,
         end: _seed.Period1.End,
         ct: CancellationToken.None
      );

      Assert.True(result.IsSuccess);
      Assert.Null(result.Value);
   }

   // ------------------------------------------------------------
   // Fake clock
   // ------------------------------------------------------------

   private sealed class FakeClock : IClock {
      public FakeClock(DateTimeOffset utcNow) => UtcNow = utcNow;
      public DateTimeOffset UtcNow { get; }
   }
}
