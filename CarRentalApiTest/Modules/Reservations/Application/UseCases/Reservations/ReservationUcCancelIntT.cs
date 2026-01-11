using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Application.UseCases;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.Errors;
using CarRentalApi.Modules.Bookings.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Modules.Reservations.Application.UseCases.Reservations;

public sealed class ReservationUcCancelIntT : TestBase, IAsyncLifetime {
   private TestSeed _seed = null!;
   private SqliteConnection _dbConnection = null!;
   private CarRentalDbContext _dbContext = null!;
   private ReservationRepositoryEf _repositoryEf = null!;
   private IUnitOfWork _unitOfWork = null!;

   private FakeClock _clock = null!;
   private ReservationUcCancel _uc = null!; // <-- your real UC

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

      // Seed related entities FIRST
      if (_seed.Customers?.Any() == true) {
         _dbContext.Customers.AddRange(_seed.Customers);
      }
      if (_seed.Cars?.Any() == true) {
         _dbContext.Cars.AddRange(_seed.Cars);
      }
      await _dbContext.SaveChangesAsync();
      
      
      _repositoryEf = new ReservationRepositoryEf(_dbContext, CreateLogger<ReservationRepositoryEf>());
      _unitOfWork = new UnitOfWork(_dbContext, CreateLogger<UnitOfWork>());

      _clock = new FakeClock(DateTimeOffset.Parse("2026-01-01T00:20:00+00:00"));

      // NOTE: ctor might differ in your codebase -> adjust if needed
      _uc = new ReservationUcCancel(
         _repositoryEf,
         _unitOfWork,
         CreateLogger<ReservationUcCancel>(),
         _clock
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
   public async Task ExecuteAsync_cancels_draft_reservation() {
      // Arrange
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync("seed", CancellationToken.None);
      _unitOfWork.ClearChangeTracker();

      var reservationId = Guid.Parse(_seed.Reservation1Id);
      _clock.UtcNow = DateTimeOffset.Parse("2026-01-01T00:20:00+00:00");

      // Act
      var result = await _uc.ExecuteAsync(reservationId, CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);

      _unitOfWork.ClearChangeTracker();
      var actual = await _repositoryEf.FindByIdAsync(reservationId, CancellationToken.None);

      Assert.NotNull(actual);
      Assert.Equal(ReservationStatus.Cancelled, actual!.Status);
      Assert.Equal(_clock.UtcNow, actual.CancelledAt);
   }

   [Fact]
   public async Task ExecuteAsync_cancels_confirmed_reservation() {
      // Arrange
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync("seed", CancellationToken.None);
      _unitOfWork.ClearChangeTracker();

      // Load from DB to get tracked entity
      var reservationId = Guid.Parse(_seed.Reservation1Id);
      var reservation = await _repositoryEf.FindByIdAsync(reservationId, CancellationToken.None);
      Assert.NotNull(reservation);

      // Confirm first (domain allows cancel Draft or Confirmed)
      var confirmAt = DateTimeOffset.Parse("2026-01-01T00:10:00+00:00");
      Assert.True(reservation!.Confirm(confirmAt).IsSuccess);

      await _unitOfWork.SaveAllChangesAsync("confirm", CancellationToken.None);
      _unitOfWork.ClearChangeTracker();

      _clock.UtcNow = DateTimeOffset.Parse("2026-01-01T00:20:00+00:00");

      // Act
      var result = await _uc.ExecuteAsync(reservationId, CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);

      _unitOfWork.ClearChangeTracker();
      var actual = await _repositoryEf.FindByIdAsync(reservationId, CancellationToken.None);

      Assert.NotNull(actual);
      Assert.Equal(ReservationStatus.Cancelled, actual!.Status);
      Assert.Equal(_clock.UtcNow, actual.CancelledAt);
   }


   [Fact]
   public async Task ExecuteAsync_returns_not_found_when_reservation_missing() {
      // Arrange
      var missingId = Guid.Parse("00000000-0000-0000-0000-000000000999");

      // Act
      var result = await _uc.ExecuteAsync(missingId, CancellationToken.None);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.NotFound.Code, result.Error.Code);
   }

   // -------------------------------------------------------------------------
   // Fake clock
   // -------------------------------------------------------------------------
   private sealed class FakeClock : IClock {
      public FakeClock(DateTimeOffset utcNow) => UtcNow = utcNow;
      public DateTimeOffset UtcNow { get; set; }
   }
}