using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Modules.Reservations.Application.UseCases;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
using CarRentalApi.Modules.Reservations.Domain.Enums;
using CarRentalApi.Modules.Reservations.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
namespace CarRentalApiTest.Modules.Reservations.Application.UseCases.Reservations.Moq;

public sealed class ReservationUcExpireMoqT {
   private readonly FakeClock _clock;
   private readonly Mock<ILogger<ReservationUcExpire>> _logger = new();
   private readonly Mock<IReservationRepository> _repo = new(MockBehavior.Strict);
   private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);

   public ReservationUcExpireMoqT() {
      var seed = new TestSeed();
      _clock = new FakeClock(seed.FixedNow);
   }

   [Fact]
   public async Task ExecuteAsync_returns_0_when_no_drafts_found_and_saves() {
      // Arrange
      _repo.Setup(r => r.SelectDraftsToExpireAsync(_clock.UtcNow, It.IsAny<CancellationToken>()))
         .ReturnsAsync(new List<Reservation>());

      _uow.Setup(u => u.SaveAllChangesAsync("Expired reservation", It.IsAny<CancellationToken>()))
         .ReturnsAsync(true);

      var sut = new ReservationUcExpire(
         _repository: _repo.Object,
         _unitOfWork: _uow.Object,
         _logger: _logger.Object,
         _clock: _clock
      );

      // Act
      var result = await sut.ExecuteAsync(CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(0, result.Value);

      _repo.VerifyAll();
      _uow.VerifyAll();
   }

   [Fact]
   public async Task ExecuteAsync_expires_all_drafts_and_returns_count_and_saves() {
      // Arrange
      // Two valid draft reservations -> both should expire
      var r1 = Reservation.Create(
         customerId: Guid.Parse("00000000-0001-0000-0000-000000000000"),
         carCategory: CarCategory.Compact,
         start: DateTimeOffset.Parse("2030-05-01T10:00:00+00:00"),
         end: DateTimeOffset.Parse("2030-05-05T10:00:00+00:00"),
         createdAt: _clock.UtcNow.AddDays(-10),
         id: "00100000-0000-0000-0000-000000000000"
      ).Value;

      var r2 = Reservation.Create(
         customerId: Guid.Parse("00000000-0002-0000-0000-000000000000"),
         carCategory: CarCategory.Compact,
         start: DateTimeOffset.Parse("2030-05-11T10:00:00+00:00"),
         end: DateTimeOffset.Parse("2030-05-15T10:00:00+00:00"),
         createdAt: _clock.UtcNow.AddDays(-9),
         id: "00200000-0000-0000-0000-000000000000"
      ).Value;

      var drafts = new List<Reservation> { r1, r2 };

      _repo.Setup(r => r.SelectDraftsToExpireAsync(_clock.UtcNow, It.IsAny<CancellationToken>()))
         .ReturnsAsync(drafts);

      _uow.Setup(u => u.SaveAllChangesAsync("Expired reservation", It.IsAny<CancellationToken>()))
         .ReturnsAsync(true);

      var sut = new ReservationUcExpire(
         _repository: _repo.Object,
         _unitOfWork: _uow.Object,
         _logger: _logger.Object,
         _clock: _clock
      );

      // Act
      var result = await sut.ExecuteAsync(CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(2, result.Value);

      Assert.Equal(ReservationStatus.Expired, r1.Status);
      Assert.Equal(_clock.UtcNow, r1.ExpiredAt);

      Assert.Equal(ReservationStatus.Expired, r2.Status);
      Assert.Equal(_clock.UtcNow, r2.ExpiredAt);

      _repo.VerifyAll();
      _uow.VerifyAll();
   }

   [Fact]
   public async Task ExecuteAsync_skips_non_draft_reservations_and_returns_only_expired_count() {
      // Arrange
      // Draft -> will expire
      var draft = Reservation.Create(
         customerId: Guid.Parse("00000000-0003-0000-0000-000000000000"),
         carCategory: CarCategory.Compact,
         start: DateTimeOffset.Parse("2030-06-01T10:00:00+00:00"),
         end: DateTimeOffset.Parse("2030-06-05T10:00:00+00:00"),
         createdAt: _clock.UtcNow.AddDays(-10),
         id: "00300000-0000-0000-0000-000000000000"
      ).Value;

      // Confirmed -> Expire(now) should fail (domain: only Draft can expire)
      var confirmed = Reservation.Create(
         customerId: Guid.Parse("00000000-0004-0000-0000-000000000000"),
         carCategory: CarCategory.Compact,
         start: DateTimeOffset.Parse("2030-07-01T10:00:00+00:00"),
         end: DateTimeOffset.Parse("2030-07-05T10:00:00+00:00"),
         createdAt: _clock.UtcNow.AddDays(-20),
         id: "00400000-0000-0000-0000-000000000000"
      ).Value;

      var confirmResult = confirmed.Confirm(_clock.UtcNow); 
      Assert.True(confirmResult.IsSuccess);
      Assert.Equal(ReservationStatus.Confirmed, confirmed.Status);

      var drafts = new List<Reservation> { draft, confirmed };

      _repo.Setup(r => r.SelectDraftsToExpireAsync(_clock.UtcNow, It.IsAny<CancellationToken>()))
         .ReturnsAsync(drafts);

      _uow.Setup(u => u.SaveAllChangesAsync("Expired reservation", It.IsAny<CancellationToken>()))
         .ReturnsAsync(true);

      var sut = new ReservationUcExpire(
         _repository: _repo.Object,
         _unitOfWork: _uow.Object,
         _logger: _logger.Object,
         _clock: _clock
      );

      // Act
      var result = await sut.ExecuteAsync(CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(1, result.Value);

      Assert.Equal(ReservationStatus.Expired, draft.Status);
      Assert.Equal(_clock.UtcNow, draft.ExpiredAt);

      // Still confirmed
      Assert.Equal(ReservationStatus.Confirmed, confirmed.Status);

      _repo.VerifyAll();
      _uow.VerifyAll();
   }
}