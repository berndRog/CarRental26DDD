using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.BuildingBlocks.Utils;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Reservations.Application;
using CarRentalApi.Modules.Reservations.Application.UseCases;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
using CarRentalApi.Modules.Reservations.Domain.Enums;
using CarRentalApi.Modules.Reservations.Domain.Errors;
using CarRentalApi.Modules.Reservations.Infrastructure;
using CarRentalApiTest.Domain.Utils;
using Microsoft.Extensions.Logging;
using Moq;
namespace CarRentalApiTest.Domain.UseCases.Reservations;

public sealed class ReservationUcCancelUt {
   private readonly Mock<IReservationRepository> _repo = new(MockBehavior.Strict);
   private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);
   private readonly Mock<ILogger<ReservationUcCancel>> _logger = new();
   private readonly FakeClock _clock;

   public ReservationUcCancelUt() {
      var seed = new TestSeed();
      _clock = new FakeClock(seed.Now);
   }

   [Fact]
   public async Task ExecuteAsync_returns_NotFound_when_reservation_missing() {
      // Arrange
      var reservationId = Guid.NewGuid();

      _repo.Setup(r => r.FindByIdAsync(reservationId, It.IsAny<CancellationToken>()))
         .ReturnsAsync((Reservation?)null);

      var sut = new ReservationUcCancel(
         _repository: _repo.Object,
         _unitOfWork: _uow.Object,
         _logger: _logger.Object,
         _clock: _clock
      );

      // Act
      var result = await sut.ExecuteAsync(reservationId, CancellationToken.None);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.NotFound.Code, result.Error.Code);

      _uow.Verify(u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);

      _repo.VerifyAll();
      _uow.VerifyAll();
   }

   [Fact]
   public async Task ExecuteAsync_returns_failure_when_domain_cancel_rejects_status_transition() {
      // Arrange
      var seed = new TestSeed();

      // Create a reservation and move it to Expired so Cancel must fail
      var reservation = Reservation.Create(
         customerId: seed.Customer1Id.ToGuid(),
         carCategory: CarCategory.Compact,
         start: seed.Period1.Start,
         end: seed.Period1.End,
         createdAt: seed.Now.AddDays(-10),
         id: seed.Reservation1Id
      ).Value;

      // Expire it -> Cancel must be rejected by domain
      var expireResult = reservation.Expire(seed.Now.AddDays(-1));
      Assert.True(expireResult.IsSuccess);
      Assert.Equal(ReservationStatus.Expired, reservation.Status);

      _repo.Setup(r => r.FindByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
         .ReturnsAsync(reservation);

      var sut = new ReservationUcCancel(
         _repository: _repo.Object,
         _unitOfWork: _uow.Object,
         _logger: _logger.Object,
         _clock: _clock
      );

      // Act
      var result = await sut.ExecuteAsync(reservation.Id, CancellationToken.None);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.InvalidStatusTransition.Code, result.Error.Code);

      _uow.Verify(u => u.SaveAllChangesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);

      _repo.VerifyAll();
      _uow.VerifyAll();
   }

   [Fact]
   public async Task ExecuteAsync_success_cancels_and_saves() {
      // Arrange
      var seed = new TestSeed();

      var reservation = Reservation.Create(
         customerId: seed.Customer1Id.ToGuid(),
         carCategory: CarCategory.Compact,
         start: seed.Period1.Start,
         end: seed.Period1.End,
         createdAt: seed.Now.AddDays(-2),
         id: seed.Reservation2Id
      ).Value;

      _repo.Setup(r => r.FindByIdAsync(reservation.Id, It.IsAny<CancellationToken>()))
         .ReturnsAsync(reservation);

      _uow.Setup(u => u.SaveAllChangesAsync("Reservation cancelled", It.IsAny<CancellationToken>()))
         .ReturnsAsync(true);

      var sut = new ReservationUcCancel(
         _repository: _repo.Object,
         _unitOfWork: _uow.Object,
         _logger: _logger.Object,
         _clock: _clock
      );

      // Act
      var result = await sut.ExecuteAsync(reservation.Id, CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
      Assert.Equal(_clock.UtcNow, reservation.CancelledAt);

      _uow.Verify(u => u.SaveAllChangesAsync("Reservation cancelled", It.IsAny<CancellationToken>()), Times.Once);

      _repo.VerifyAll();
      _uow.VerifyAll();
   }
}