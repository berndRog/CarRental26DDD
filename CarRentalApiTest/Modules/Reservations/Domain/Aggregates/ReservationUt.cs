using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Utils;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.Errors;
namespace CarRentalApiTest.Modules.Reservations.Domain;

public sealed class ReservationTests {

   private readonly TestSeed _seed = new();

   [Fact]
   public void CreateDraft_success_sets_initial_state() {
      // Arrange
      var createdAt = _seed.FixedNow;
      var start = createdAt.AddDays(10);
      var end = createdAt.AddDays(12);

      // Act
      var result = Reservation.Create(
         customerId: _seed.Customer1Id.ToGuid(),
         carCategory: CarCategory.Compact,
         start: start,
         end: end,
         createdAt: createdAt,
         id: _seed.Reservation2Id
      );

      // Assert
      Assert.True(result.IsSuccess);
      var reservation = result.Value;

      Assert.Equal(_seed.Reservation2Id.ToGuid(), reservation.Id);
      Assert.Equal(_seed.Customer1Id.ToGuid(), reservation.CustomerId);
      Assert.Equal(CarCategory.Compact, reservation.CarCategory);

      Assert.Equal(ReservationStatus.Draft, reservation.Status);
      Assert.Equal(createdAt, reservation.CreatedAt);

      Assert.Equal(start, reservation.Period.Start);
      Assert.Equal(end, reservation.Period.End);

      Assert.Null(reservation.ConfirmedAt);
      Assert.Null(reservation.CancelledAt);
      Assert.Null(reservation.ExpiredAt);
   }

   [Fact]
   public void CreateDraft_invalid_period_returns_failure() {
      // Arrange
      var createdAt = _seed.FixedNow;
      var start = createdAt.AddDays(10);
      var end = start; // invalid: end == start

      // Act
      var result = Reservation.Create(
         customerId: _seed.Customer1Id.ToGuid(),
         carCategory: CarCategory.Compact,
         start: start,
         end: end,
         createdAt: createdAt,
         id: _seed.Reservation2Id
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.InvalidPeriod.Code, result.Error.Code);
   }

   [Fact]
   public void ChangePeriod_when_draft_updates_period() {
      // Arrange
      var reservation = _seed.Reservation1; // Draft
      var newStart = reservation.Period.Start.AddDays(30);
      var newEnd = reservation.Period.End.AddDays(30);

      // Act
      var result = reservation.ChangePeriod(newStart, newEnd);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(newStart, reservation.Period.Start);
      Assert.Equal(newEnd, reservation.Period.End);
   }

   [Fact]
   public void ChangePeriod_when_not_draft_returns_failure() {
      // Arrange
      var reservation = _seed.Reservation1;
      var confirmAt = reservation.CreatedAt.AddMinutes(5);
      Assert.True(reservation.Confirm(confirmAt).IsSuccess);

      // Act
      var result = reservation.ChangePeriod(
         reservation.Period.Start.AddDays(1),
         reservation.Period.End.AddDays(1)
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.InvalidStatusTransition.Code, result.Error.Code);
   }

   [Fact]
   public void Confirm_when_draft_sets_status_and_timestamp() {
      // Arrange
      var reservation = _seed.Reservation1;
      var confirmedAt = reservation.CreatedAt.AddMinutes(10);

      // Act
      var result = reservation.Confirm(confirmedAt);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
      Assert.Equal(confirmedAt, reservation.ConfirmedAt);
   }

   [Fact]
   public void Confirm_when_timestamp_before_createdAt_returns_failure() {
      // Arrange
      var reservation = _seed.Reservation1;
      var confirmedAt = reservation.CreatedAt.AddMinutes(-1);

      // Act
      var result = reservation.Confirm(confirmedAt);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.InvalidTimestamp.Code, result.Error.Code); // or InvalidTimestamp if you added it
      Assert.Null(reservation.ConfirmedAt);
      Assert.Equal(ReservationStatus.Draft, reservation.Status);
   }

   [Fact]
   public void Confirm_when_not_draft_returns_failure() {
      // Arrange
      var reservation = _seed.Reservation1;
      var confirmedAt = reservation.CreatedAt.AddMinutes(10);
      Assert.True(reservation.Confirm(confirmedAt).IsSuccess);

      // Act
      var result = reservation.Confirm(confirmedAt.AddMinutes(1));

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.InvalidStatusTransition.Code, result.Error.Code);
   }

   [Fact]
   public void Cancel_from_draft_sets_status_and_timestamp() {
      // Arrange
      var reservation = _seed.Reservation1;
      var cancelledAt = reservation.CreatedAt.AddMinutes(2);

      // Act
      var result = reservation.Cancel(cancelledAt);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
      Assert.Equal(cancelledAt, reservation.CancelledAt);
   }

   [Fact]
   public void Cancel_from_confirmed_sets_status_and_timestamp() {
      // Arrange
      var reservation = _seed.Reservation1;
      var confirmedAt = reservation.CreatedAt.AddMinutes(2);
      Assert.True(reservation.Confirm(confirmedAt).IsSuccess);

      var cancelledAt = confirmedAt.AddMinutes(1);

      // Act
      var result = reservation.Cancel(cancelledAt);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
      Assert.Equal(cancelledAt, reservation.CancelledAt);
   }

   [Fact]
   public void Cancel_when_expired_returns_failure() {
      // Arrange
      var reservation = _seed.Reservation1;
      var expiredAt = reservation.CreatedAt.AddMinutes(2);
      Assert.True(reservation.Expire(expiredAt).IsSuccess);

      // Act
      var result = reservation.Cancel(expiredAt.AddMinutes(1));

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.InvalidStatusTransition.Code, result.Error.Code);
      Assert.Equal(ReservationStatus.Expired, reservation.Status);
   }

   [Fact]
   public void Expire_from_draft_sets_status_and_timestamp() {
      // Arrange
      var reservation = _seed.Reservation1;
      var expiredAt = reservation.CreatedAt.AddMinutes(3);

      // Act
      var result = reservation.Expire(expiredAt);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(ReservationStatus.Expired, reservation.Status);
      Assert.Equal(expiredAt, reservation.ExpiredAt);
   }

   [Fact]
   public void Expire_when_not_draft_returns_failure() {
      // Arrange
      var reservation = _seed.Reservation1;
      var confirmedAt = reservation.CreatedAt.AddMinutes(3);
      Assert.True(reservation.Confirm(confirmedAt).IsSuccess);

      // Act
      var result = reservation.Expire(confirmedAt.AddMinutes(1));

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(ReservationErrors.InvalidStatusTransition.Code, result.Error.Code);
   }
}
