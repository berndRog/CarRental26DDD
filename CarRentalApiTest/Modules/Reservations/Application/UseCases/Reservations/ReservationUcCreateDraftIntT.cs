using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.BuildingBlocks.Utils;
using CarRentalApi.Data.Database;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Reservations.Application;
using CarRentalApi.Modules.Reservations.Application.UseCases;
using CarRentalApi.Modules.Reservations.Domain.Enums;
using CarRentalApi.Modules.Reservations.Domain.Errors;
using CarRentalApi.Modules.Reservations.Infrastructure;
using CarRentalApi.Modules.Reservations.Infrastructure.Repositories;
using CarRentalApiTest.Domain.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Domain.UseCases.Reservations;

public sealed class ReservationUcCreateDraftIntT : TestBase, IAsyncLifetime {
   private TestSeed _seed = null!;
   private SqliteConnection _dbConnection = null!;
   private CarRentalDbContext _dbContext = null!;
   private IReservationRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private FakeClock _clock = null!;
   private ReservationUcCreate _sut = null!;

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

      if (_seed.Customers?.Any() == true) {
         _dbContext.Customers.AddRange(_seed.Customers);
      }
      if (_seed.Cars?.Any() == true) {
         _dbContext.Cars.AddRange(_seed.Cars);
      }
      
      _repository = new ReservationRepository(_dbContext, CreateLogger<ReservationRepository>());
      _unitOfWork = new UnitOfWork(_dbContext, CreateLogger<UnitOfWork>());

      _clock = new FakeClock(_seed.Now);

      _sut = new ReservationUcCreate(
         _repository: _repository,
         _unitOfWork: _unitOfWork,
         _logger: CreateLogger<ReservationUcCreate>(),
         _clock: _clock
      );
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

   [Fact]
   public async Task ExecuteAsync_returns_StartDateInPast_when_start_is_not_in_future() {
      // Arrange
      var start = _clock.UtcNow; // not allowed: must be > now
      var end = _clock.UtcNow.AddHours(1);

      // Act
      var result = await _sut.ExecuteAsync(
         customerId: _seed.Customer1Id.ToGuid(),
         carCategory: CarCategory.Compact,
         start: start,
         end: end,
         id: _seed.Reservation1Id,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.StartDateInPast.Code, result.Error.Code);

      // DB must remain empty
      var saved = await _dbContext.Reservations.CountAsync();
      Assert.Equal(0, saved);
   }

   [Fact]
   public async Task ExecuteAsync_returns_InvalidPeriod_when_end_is_not_after_start() {
      // Arrange
      var start = _clock.UtcNow.AddDays(1);
      var end = start; // invalid period

      // Act
      var result = await _sut.ExecuteAsync(
         customerId: _seed.Customer1Id.ToGuid(),
         carCategory: CarCategory.Compact,
         start: start,
         end: end,
         id: _seed.Reservation1Id,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.InvalidPeriod.Code, result.Error.Code);

      // DB must remain empty
      var saved = await _dbContext.Reservations.CountAsync();
      Assert.Equal(0, saved);
   }

   [Fact]
   public async Task ExecuteAsync_success_adds_reservation_and_saves() {
      // Arrange
      var customerId = _seed.Customer1Id.ToGuid();
      var start = _clock.UtcNow.AddDays(10);
      var end = _clock.UtcNow.AddDays(12);
      var id = _seed.Reservation2Id;

      // Act
      var result = await _sut.ExecuteAsync(
         customerId: customerId,
         carCategory: CarCategory.Compact,
         start: start,
         end: end,
         id: id,
         ct: CancellationToken.None
      );

      // Assert (use case result)
      Assert.True(result.IsSuccess);

      var reservation = result.Value;
      Assert.Equal(Guid.Parse(id), reservation.Id);
      Assert.Equal(customerId, reservation.CustomerId);
      Assert.Equal(CarCategory.Compact, reservation.CarCategory);
      Assert.Equal(ReservationStatus.Draft, reservation.Status);

      Assert.Equal(start, reservation.Period.Start);
      Assert.Equal(end, reservation.Period.End);

      // createdAt must be the _clock's "now"
      Assert.Equal(_clock.UtcNow, reservation.CreatedAt);

      // Assert (persisted to DB)
      var fromDb = await _repository.FindByIdAsync(Guid.Parse(id), CancellationToken.None);
      Assert.NotNull(fromDb);

      Assert.Equal(Guid.Parse(id), fromDb!.Id);
      Assert.Equal(customerId, fromDb.CustomerId);
      Assert.Equal(CarCategory.Compact, fromDb.CarCategory);
      Assert.Equal(ReservationStatus.Draft, fromDb.Status);
      Assert.Equal(start, fromDb.Period.Start);
      Assert.Equal(end, fromDb.Period.End);
      Assert.Equal(_clock.UtcNow, fromDb.CreatedAt);

      var count = await _dbContext.Reservations.CountAsync();
      Assert.Equal(1, count);
   }
}