using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.BuildingBlocks.Utils;
using CarRentalApi.Modules.Reservations.Application.UseCases;
using CarRentalApi.Modules.Reservations.Domain;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
using CarRentalApi.Modules.Reservations.Domain.Enums;
using CarRentalApi.Modules.Reservations.Domain.Errors;
using CarRentalApi.Modules.Reservations.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
namespace CarRentalApiTest.Modules.Reservations.Application.UseCases.Reservations.Moq;

public sealed class ReservationUcConfirmMoqT {
   private readonly Mock<IReservationRepository> _repo = new(MockBehavior.Strict);
   private readonly Mock<IReservationConflictPolicy> _conflicts = new(MockBehavior.Strict);
   private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);
   private readonly Mock<ILogger<ReservationUcConfirm>> _logger = new();

   [Fact]
   public async Task ExecuteAsync_returns_NotFound_when_reservation_missing() {
      // Arrange
      var seed = new TestSeed();
      var reservationId = Guid.NewGuid();

      _repo
         .Setup(r => r.FindByIdAsync(reservationId, It.IsAny<CancellationToken>()))
         .ReturnsAsync((Reservation?)null);

      // Important:
      // NotFound must NOT save anything, so we do not set up SaveAllChangesAsync here.
      var clock = new FakeClock(seed.FixedNow);

      var sut = new ReservationUcConfirm(
         _repository: _repo.Object,
         _conflicts: _conflicts.Object,
         _unitOfWork: _uow.Object,
         _logger: _logger.Object,
         _clock: clock
      );

      // Act
      var result = await sut.ExecuteAsync(reservationId, CancellationToken.None);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.NotFound.Code, result.Error.Code);

      _conflicts.VerifyNoOtherCalls();
      _uow.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );

      _repo.VerifyAll();
      _uow.VerifyAll();
   }

   [Fact]
   public async Task ExecuteAsync_returns_conflict_error_when_policy_detects_overcapacity() {
      // Arrange
      var seed = new TestSeed();

      var createdAt = DateTimeOffset.Parse("2025-12-25T10:00:00+00:00");
      var start = DateTimeOffset.Parse("2030-05-01T10:00:00+00:00");
      var end = DateTimeOffset.Parse("2030-05-05T10:00:00+00:00");

      var reservation = Reservation.Create(
         customerId: seed.Customer1Id.ToGuid(),
         carCategory: CarCategory.Compact,
         start: start,
         end: end,
         createdAt: createdAt,
         id: seed.Reservation1Id
      ).Value;

      _repo
         .Setup(r => r.FindByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
         .ReturnsAsync(reservation);

      _conflicts
         .Setup(c => c.CheckAsync(
            reservation.CarCategory,
            reservation.Period,
            reservation.Id,
            It.IsAny<CancellationToken>()))
         .ReturnsAsync(ReservationConflict.OverCapacity);

      var clock = new FakeClock(seed.FixedNow);

      var sut = new ReservationUcConfirm(
         _repository: _repo.Object,
         _conflicts: _conflicts.Object,
         _unitOfWork: _uow.Object,
         _logger: _logger.Object,
         _clock: clock
      );

      // Act
      var result = await sut.ExecuteAsync(reservation.Id, CancellationToken.None);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.Conflict.Code, result.Error.Code);

      _uow.Verify(
         u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()),
         Times.Never
      );

      _repo.VerifyAll();
      _conflicts.VerifyAll();
      _uow.VerifyAll();
   }

   [Fact]
   public async Task ExecuteAsync_success_confirms_and_saves() {
      // Arrange
      var seed = new TestSeed();
      var clock = new FakeClock(DateTimeOffset.Parse("2026-01-01T00:00:00+00:00"));

      var reservation = Reservation.Create(
         customerId: seed.Customer1Id.ToGuid(),
         carCategory: CarCategory.Compact,
         start: DateTimeOffset.Parse("2030-05-01T10:00:00+00:00"),
         end: DateTimeOffset.Parse("2030-05-05T10:00:00+00:00"),
         createdAt: DateTimeOffset.Parse("2025-12-25T10:00:00+00:00"),
         id: seed.Reservation1Id
      ).Value;

      _repo
         .Setup(r => r.FindByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
         .ReturnsAsync(reservation);

      _conflicts
         .Setup(c => c.CheckAsync(
            reservation.CarCategory,
            reservation.Period,
            reservation.Id,
            It.IsAny<CancellationToken>()))
         .ReturnsAsync(ReservationConflict.None);

      _uow
         .Setup(u => u.SaveAllChangesAsync("Reservation confirmed", It.IsAny<CancellationToken>()))
         .ReturnsAsync(true);

      var sut = new ReservationUcConfirm(
         _repository: _repo.Object,
         _conflicts: _conflicts.Object,
         _unitOfWork: _uow.Object,
         _logger: _logger.Object,
         _clock: clock
      );

      // Act
      var result = await sut.ExecuteAsync(reservation.Id, CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
      Assert.Equal(clock.UtcNow, reservation.ConfirmedAt);

      _uow.Verify(
         u => u.SaveAllChangesAsync("Reservation confirmed", It.IsAny<CancellationToken>()),
         Times.Once
      );

      _repo.VerifyAll();
      _conflicts.VerifyAll();
      _uow.VerifyAll();
   }
}