using CarRentalApi._2_Modules.Bookings._2_Application.UseCases.Reservation;
using CarRentalApi._2_Modules.Bookings._3_Domain.Enums;
using CarRentalApi._2_Modules.Bookings._3_Domain.Errors;
using CarRentalApi._2_Modules.Bookings._3_Domain.Policies;
using CarRentalApi._2_Modules.Bookings._3_Domain.ValueObjects;
using CarRentalApi._2_Modules.Bookings._4_Infrastructure.Repositories;
using CarRentalApi._3_Infrastructure.Persistence.Database;
using CarRentalApi._4_BuildingBlocks._1_Ports.Inbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Domain.UseCases.Reservations;

public sealed class ReservationUcConfirmIt : TestBase, IAsyncLifetime {
   private TestSeed _seed = null!;
   private SqliteConnection _dbConnection = null!;
   private CarRentalDbContext _dbContext = null!;
   private ReservationRepositoryEf _repositoryEf = null!;
   private IUnitOfWork _unitOfWork = null!;

   private FakeConflictPolicy _conflicts = null!;
   private FakeClock _clock = null!;
   private ReservationUcConfirm _uc = null!;

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
      
      _repositoryEf = new ReservationRepositoryEf(_dbContext, CreateLogger<ReservationRepositoryEf>());
      _unitOfWork = new UnitOfWork(_dbContext, CreateLogger<UnitOfWork>());

      _conflicts = new FakeConflictPolicy();
      _clock = new FakeClock(DateTimeOffset.Parse("2026-01-01T00:10:00+00:00"));

      _uc = new ReservationUcConfirm(
         _repositoryEf,
         _unitOfWork,
         _conflicts,
         CreateLogger<ReservationUcConfirm>(),
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
   public async Task ExecuteAsync_confirms_reservation_when_no_conflict() {
      // Arrange: persist raw seed (all Draft initially)
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync("seed", CancellationToken.None);
      _unitOfWork.ClearChangeTracker();

      var reservationId = Guid.Parse(_seed.Reservation1Id);

      _conflicts.NextConflict = ReservationConflict.None;
      _clock.UtcNow = DateTimeOffset.Parse("2026-01-01T00:10:00+00:00");

      // Act
      var result = await _uc.ExecuteAsync(reservationId, CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);

      _unitOfWork.ClearChangeTracker();
      var actual = await _repositoryEf.FindByIdAsync(reservationId, CancellationToken.None);

      Assert.NotNull(actual);
      Assert.Equal(ReservationStatus.Confirmed, actual!.Status);
      Assert.Equal(_clock.UtcNow, actual.ConfirmedAt);
   }

   [Fact]
   public async Task ExecuteAsync_returns_failure_when_conflict_exists_and_does_not_confirm() {
      // Arrange
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync("seed", CancellationToken.None);
      _unitOfWork.ClearChangeTracker();

      var reservationId = Guid.Parse(_seed.Reservation1Id);

      // simulate conflict (your UC maps it via Reservation.MapConflict)
      _conflicts.NextConflict = ReservationConflict.OverCapacity;
      _clock.UtcNow = DateTimeOffset.Parse("2026-01-01T00:10:00+00:00");

      // Act
      var result = await _uc.ExecuteAsync(reservationId, CancellationToken.None);

      // Assert
      Assert.True(result.IsFailure);
      Assert.NotNull(result.Error);

      _unitOfWork.ClearChangeTracker();
      var actual = await _repositoryEf.FindByIdAsync(reservationId, CancellationToken.None);

      Assert.NotNull(actual);
      Assert.Equal(ReservationStatus.Draft, actual!.Status);
      Assert.Null(actual.ConfirmedAt);
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
   // Fakes
   // -------------------------------------------------------------------------

   private sealed class FakeClock : IClock {
      public FakeClock(DateTimeOffset utcNow) => UtcNow = utcNow;
      public DateTimeOffset UtcNow { get; set; }
   }

   private sealed class FakeConflictPolicy : IReservationConflictPolicy {
      public ReservationConflict NextConflict { get; set; } = ReservationConflict.None;

      public Task<ReservationConflict> CheckAsync(
         CarCategory carCategory,
         RentalPeriod period,
         Guid ignoreReservationId,
         CancellationToken ct
      ) {
         ct.ThrowIfCancellationRequested();
         return Task.FromResult(NextConflict);
      }
   }
}