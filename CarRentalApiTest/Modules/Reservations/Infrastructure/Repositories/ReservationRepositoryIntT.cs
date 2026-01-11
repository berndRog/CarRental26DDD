using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Infrastructure;
using CarRentalApi.Modules.Bookings.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Modules.Reservations.Infrastructure.Repositories;

public sealed class ReservationRepositoryIt : TestBase, IAsyncLifetime {
   private TestSeed _seed = null!;
   private SqliteConnection _dbConnection = null!;
   private CarRentalDbContext _dbContext = null!;
   private IReservationRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;

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

      _repository = new ReservationRepositoryEf(_dbContext, CreateLogger<ReservationRepositoryEf>());
      _unitOfWork = new UnitOfWork(_dbContext, CreateLogger<UnitOfWork>());
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
   public async Task FindByIdAsync_returns_reservation_when_found() {
      // Arrange
      _dbContext.Customers.AddRange(_seed.Customers);
      await _unitOfWork.SaveAllChangesAsync();
      
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear(); // Make assertions more realistic

      var id = Guid.Parse(_seed.Reservation1Id);

      // Act
      var actual = await _repository.FindByIdAsync(id, CancellationToken.None);

      // Assert
      Assert.NotNull(actual);
      Assert.Equal(id, actual!.Id);
   }
   
   [Fact]
   public async Task FindByIdAsync_returns_null_when_not_found() {
      // Arrange
      _dbContext.Customers.AddRange(_seed.Customers);
      await _unitOfWork.SaveAllChangesAsync();
      
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();
   
      var nonExistentId = Guid.NewGuid();
   
      // Act
      var actual = await _repository.FindByIdAsync(nonExistentId, CancellationToken.None);
   
      // Assert
      Assert.Null(actual);
   }
   

   [Fact]
   public async Task CountConfirmedOverlappingAsync_counts_only_confirmed_overlapping_and_ignores_given_id() {
      // Arrange
      var category = CarCategory.Compact;
      var start = _seed.Period1.Start;
      var end = _seed.Period1.End;

      // Persist customers first (required for FK constraint)
      _dbContext.Customers.AddRange(_seed.Customers);
      await _unitOfWork.SaveAllChangesAsync();

      // Persist seed reservations (they are Draft initially)
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync();

      // Confirm 1..9 (10 stays Draft)
      var confirmAt = DateTimeOffset.Parse("2026-01-01T00:10:00+00:00");

      // 1..8 are overlapping, 9 is confirmed but non-overlapping, 10 stays draft
      foreach (var r in new[] {
                  _seed.Reservation1, _seed.Reservation2, _seed.Reservation3, _seed.Reservation4,
                  _seed.Reservation5, _seed.Reservation6, _seed.Reservation7, _seed.Reservation8,
                  _seed.Reservation9
               }) {
         var res = r.Confirm(confirmAt);
         Assert.True(res.IsSuccess);
      }

      await _dbContext.SaveChangesAsync();

      var ignoreId = Guid.Parse(_seed.Reservation1Id);

      // Act
      var count = await _repository.CountConfirmedOverlappingAsync(
         category: category,
         start: start,
         end: end,
         ignoreReservationId: ignoreId,
         ct: CancellationToken.None
      );

      // Assert
      // 8 overlapping confirmed - 1 ignored = 7
      Assert.Equal(7, count);
   }
   
[Fact]
   public async Task CountConfirmedOverlappingAsync_returns_zero_when_no_overlapping() {
      // Arrange
      _dbContext.Customers.AddRange(_seed.Customers);
      await _unitOfWork.SaveAllChangesAsync();
   
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync();
   
      var confirmAt = DateTimeOffset.Parse("2026-01-01T00:10:00+00:00");
      foreach (var r in _seed.Reservations) {
         r.Confirm(confirmAt);
      }
      await _unitOfWork.SaveAllChangesAsync();
   
      var category = CarCategory.Compact;
      // Period that does not overlap with any reservation
      var start = DateTimeOffset.Parse("2035-01-01T10:00:00+00:00");
      var end = DateTimeOffset.Parse("2035-01-05T10:00:00+00:00");
   
      // Act
      var count = await _repository.CountConfirmedOverlappingAsync(
         category: category,
         start: start,
         end: end,
         ignoreReservationId: Guid.Empty,
         ct: CancellationToken.None
      );
   
      // Assert
      Assert.Equal(0, count);
   }
   
   [Fact]
   public async Task CountConfirmedOverlappingAsync_excludes_draft_reservations() {
      // Arrange
      _dbContext.Customers.AddRange(_seed.Customers);
      await _unitOfWork.SaveAllChangesAsync();
   
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync();
   
      // Do NOT confirm any reservations (all stay Draft)
      var category = CarCategory.Compact;
      var start = _seed.Period1.Start;
      var end = _seed.Period1.End;
   
      // Act
      var count = await _repository.CountConfirmedOverlappingAsync(
         category: category,
         start: start,
         end: end,
         ignoreReservationId: Guid.Empty,
         ct: CancellationToken.None
      );
   
      // Assert
      Assert.Equal(0, count); // No confirmed reservations
   }
   
   [Fact]
   public async Task CountConfirmedOverlappingAsync_excludes_different_category() {
      // Arrange
      _dbContext.Customers.AddRange(_seed.Customers);
      await _unitOfWork.SaveAllChangesAsync();
   
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync();
   
      var confirmAt = DateTimeOffset.Parse("2026-01-01T00:10:00+00:00");
      foreach (var r in _seed.Reservations) {
         r.Confirm(confirmAt);
      }
      await _unitOfWork.SaveAllChangesAsync();
   
      // Query for a different category (all reservations are Compact)
      var category = CarCategory.Economy;
      var start = _seed.Period1.Start;
      var end = _seed.Period1.End;
   
      // Act
      var count = await _repository.CountConfirmedOverlappingAsync(
         category: category,
         start: start,
         end: end,
         ignoreReservationId: Guid.Empty,
         ct: CancellationToken.None
      );
   
      // Assert
      Assert.Equal(0, count);
   }
   
   [Fact]
   public async Task SelectDraftsToExpireAsync_returns_only_drafts_created_before_or_at_now() {
      // Arrange
      _dbContext.Customers.AddRange(_seed.Customers);
      await _unitOfWork.SaveAllChangesAsync();

      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync();

      // Make Reservation1..9 confirmed so they are not returned
      var confirmAt = DateTimeOffset.Parse("2026-01-01T00:10:00+00:00");
      foreach (var r in new[] {
                  _seed.Reservation1, _seed.Reservation2, _seed.Reservation3, _seed.Reservation4,
                  _seed.Reservation5, _seed.Reservation6, _seed.Reservation7, _seed.Reservation8,
                  _seed.Reservation9
               }) {
         Assert.True(r.Confirm(confirmAt).IsSuccess);
      }
      await _unitOfWork.SaveAllChangesAsync("confirm for test", CancellationToken.None);
      _unitOfWork.ClearChangeTracker();

      var now = _seed.FixedNow;

      // Act
      var drafts = await _repository.SelectDraftsToExpireAsync(now, CancellationToken.None);

      // Assert
      Assert.Single(drafts);
      Assert.Equal(Guid.Parse(_seed.Reservation10Id), drafts[0].Id);
      Assert.Equal(ReservationStatus.Draft, drafts[0].Status);
   }
   
[Fact]
   public async Task SelectDraftsToExpireAsync_returns_empty_when_no_drafts() {
      // Arrange
      _dbContext.Customers.AddRange(_seed.Customers);
      await _unitOfWork.SaveAllChangesAsync();
   
      _dbContext.Reservations.AddRange(_seed.Reservations);
      await _unitOfWork.SaveAllChangesAsync();
   
      // Confirm all reservations
      var confirmAt = DateTimeOffset.Parse("2026-01-01T00:10:00+00:00");
      foreach (var r in _seed.Reservations) {
         r.Confirm(confirmAt);
      }
      await _unitOfWork.SaveAllChangesAsync();
   
      var now = _seed.FixedNow;
   
      // Act
      var drafts = await _repository.SelectDraftsToExpireAsync(now, CancellationToken.None);
   
      // Assert
      Assert.Empty(drafts);
   }
   
   [Fact]
   public async Task SelectDraftsToExpireAsync_excludes_drafts_created_after_now() {
      // Arrange
      _dbContext.Customers.AddRange(_seed.Customers);
      await _unitOfWork.SaveAllChangesAsync();
   
      // Only add Reservation10 (created in the past)
      _dbContext.Reservations.Add(_seed.Reservation10);
      await _unitOfWork.SaveAllChangesAsync();
   
      // Set 'now' to be BEFORE Reservation10.CreatedAt
      var now = _seed.DraftCreatedAtOld.AddDays(-1);
   
      // Act
      var drafts = await _repository.SelectDraftsToExpireAsync(now, CancellationToken.None);
   
      // Assert
      Assert.Empty(drafts);
   }
   
}
