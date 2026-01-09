using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Modules.Rentals.Domain.Enums;
using CarRentalApi.Modules.Rentals.Domain.Errors;
namespace CarRentalApiTest.Modules.Rentals.Domain.Aggregates;

public class RentalTests {

   private readonly TestSeed _seed = new();

   [Fact]
   public void CreateAtPickup_WithValidData_ShouldSucceed() {
      // Arrange
      var reservationId = Guid.NewGuid();
      var customerId = Guid.NewGuid();
      var carId = Guid.NewGuid();
      var pickupAt = DateTimeOffset.Now;
      const int fuelLevelOut = 80;
      const int kmOut = 50000;

      // Act
      var result = Rental.CreateAtPickup(
         reservationId,
         customerId,
         carId,
         pickupAt,
         fuelLevelOut,
         kmOut
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(reservationId, result.Value.ReservationId);
      Assert.Equal(customerId, result.Value.CustomerId);
      Assert.Equal(carId, result.Value.CarId);
      Assert.Equal(pickupAt, result.Value.PickupAt);
      Assert.Equal(fuelLevelOut, result.Value.FuelLevelOut);
      Assert.Equal(kmOut, result.Value.KmOut);
      Assert.Equal(RentalStatus.Active, result.Value.Status);
      Assert.Null(result.Value.ReturnAt);
      Assert.Null(result.Value.FuelLevelIn);
      Assert.Null(result.Value.KmIn);
   }

   [Fact]
   public void CreateAtPickup_WithEmptyReservationId_ShouldFail() {
      // Act
      var result = Rental.CreateAtPickup(
         Guid.Empty,
         Guid.NewGuid(),
         Guid.NewGuid(),
         DateTimeOffset.Now,
         80,
         50000
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(RentalErrors.InvalidReservation, result.Error);
   }

   [Fact]
   public void CreateAtPickup_WithEmptyCustomerId_ShouldFail() {
      // Act
      var result = Rental.CreateAtPickup(
         Guid.NewGuid(),
         Guid.Empty,
         Guid.NewGuid(),
         DateTimeOffset.Now,
         80,
         50000
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(RentalErrors.InvalidCustomer, result.Error);
   }

   [Fact]
   public void CreateAtPickup_WithEmptyCarId_ShouldFail() {
      // Act
      var result = Rental.CreateAtPickup(
         Guid.NewGuid(),
         Guid.NewGuid(),
         Guid.Empty,
         DateTimeOffset.Now,
         80,
         50000
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(RentalErrors.InvalidCar, result.Error);
   }

   [Theory]
   [InlineData(-1)]
   [InlineData(101)]
   [InlineData(150)]
   public void CreateAtPickup_WithInvalidFuelLevel_ShouldFail(int fuelLevel) {
      // Act
      var result = Rental.CreateAtPickup(
         Guid.NewGuid(),
         Guid.NewGuid(),
         Guid.NewGuid(),
         DateTimeOffset.Now,
         fuelLevel,
         50000
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(RentalErrors.InvalidFuelLevel, result.Error);
   }

   [Theory]
   [InlineData(-1)]
   [InlineData(-100)]
   public void CreateAtPickup_WithNegativeKm_ShouldFail(int km) {
      // Act
      var result = Rental.CreateAtPickup(
         Guid.NewGuid(),
         Guid.NewGuid(),
         Guid.NewGuid(),
         DateTimeOffset.Now,
         80,
         km
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(RentalErrors.InvalidKm, result.Error);
   }

   [Fact]
   public void ReturnCar_WithValidData_ShouldSucceed() {
      // Arrange
      var rental = _seed.Rental1;
      var returnAt = rental.PickupAt.AddDays(3);
      const int fuelLevelIn = 70;
      const int kmIn = 50500;

      // Act
      var result = rental.ReturnCar(returnAt, fuelLevelIn, kmIn);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(RentalStatus.Returned, rental.Status);
      Assert.Equal(returnAt, rental.ReturnAt);
      Assert.Equal(fuelLevelIn, rental.FuelLevelIn);
      Assert.Equal(kmIn, rental.KmIn);
   }

   [Fact]
   public void ReturnCar_WithReturnBeforePickup_ShouldFail() {
      // Arrange
      var rental = _seed.Rental1;
      var returnAt = rental.PickupAt.AddDays(-1); // Before pickup

      // Act
      var result = rental.ReturnCar(returnAt, 70, 50500);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(RentalErrors.InvalidTimestamp, result.Error);
      Assert.Equal(RentalStatus.Active, rental.Status);
   }

   [Theory]
   [InlineData(-1)]
   [InlineData(101)]
   public void ReturnCar_WithInvalidFuelLevel_ShouldFail(int fuelLevel) {
      // Arrange
      var rental = _seed.Rental1;
      var returnAt = rental.PickupAt.AddDays(3);

      // Act
      var result = rental.ReturnCar(returnAt, fuelLevel, 50500);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(RentalErrors.InvalidFuelLevel, result.Error);
   }

   [Fact]
   public void ReturnCar_WithKmLowerThanOut_ShouldFail() {
      // Arrange
      var rental = _seed.Rental1;
      var returnAt = rental.PickupAt.AddDays(3);
      var kmIn = rental.KmOut - 100; // Lower than KmOut

      // Act
      var result = rental.ReturnCar(returnAt, 70, kmIn);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(RentalErrors.InvalidKm, result.Error);
   }

   [Fact]
   public void ReturnCar_WhenAlreadyReturned_ShouldFail() {
      // Arrange
      var rental = _seed.Rental1;
      rental.ReturnCar(rental.PickupAt.AddDays(3), 70, 50500);

      // Act
      var result = rental.ReturnCar(rental.PickupAt.AddDays(4), 60, 50600);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(RentalErrors.InvalidStatusTransition, result.Error);
   }

   [Fact]
   public void IsReturned_WhenActive_ShouldReturnFalse() {
      // Arrange
      var rental = _seed.Rental1;

      // Act & Assert
      Assert.False(rental.IsReturned());
   }

   [Fact]
   public void IsReturned_WhenReturned_ShouldReturnTrue() {
      // Arrange
      var rental = _seed.Rental1;
      rental.ReturnCar(rental.PickupAt.AddDays(3), 70, 50500);

      // Act & Assert
      Assert.True(rental.IsReturned());
   }

   [Fact]
   public void NeedsRefuelFee_WhenNotReturned_ShouldReturnFalse() {
      // Arrange
      var rental = _seed.Rental1;

      // Act & Assert
      Assert.False(rental.NeedsRefuelFee());
   }

   [Fact]
   public void NeedsRefuelFee_WhenFuelLevelDecreased_ShouldReturnTrue() {
      // Arrange
      var rental = _seed.Rental1; // FuelLevelOut = 80
      rental.ReturnCar(rental.PickupAt.AddDays(3), 70, 50500); // FuelLevelIn < FuelLevelOut

      // Act & Assert
      Assert.True(rental.NeedsRefuelFee());
   }

   [Fact]
   public void NeedsRefuelFee_WhenFuelLevelSameOrHigher_ShouldReturnFalse() {
      // Arrange
      var rental = _seed.Rental1; // FuelLevelOut = 80
      rental.ReturnCar(rental.PickupAt.AddDays(3), 80, 50500); // FuelLevelIn = FuelLevelOut

      // Act & Assert
      Assert.False(rental.NeedsRefuelFee());
   }

   [Fact]
   public void Equals_WithSameId_ShouldBeTrue() {
      // Arrange
      var rental1 = _seed.Rental1;
      var rental2 = Rental.CreateAtPickup(
         Guid.NewGuid(),
         Guid.NewGuid(),
         Guid.NewGuid(),
         DateTimeOffset.Now,
         100,
         0,
         _seed.Rental1Id
      ).GetValueOrThrow();

      // Act & Assert
      Assert.Equal(rental1, rental2);
      Assert.Equal(rental1.GetHashCode(), rental2.GetHashCode());
   }

   [Fact]
   public void Equals_WithDifferentId_ShouldBeFalse() {
      // Arrange
      var rental1 = _seed.Rental1;
      var rental2 = _seed.Rental2;

      // Act & Assert
      Assert.NotEqual(rental1, rental2);
   }

   [Fact]
   public void TestSeed_AllRentals_ShouldBeValid() {
      // Assert
      Assert.Equal(RentalStatus.Active, _seed.Rental1.Status);
      Assert.Equal(80, _seed.Rental1.FuelLevelOut);
      Assert.Equal(10000, _seed.Rental1.KmOut);

      Assert.Equal(RentalStatus.Active, _seed.Rental2.Status);
      Assert.Equal(70, _seed.Rental2.FuelLevelOut);
      Assert.Equal(20000, _seed.Rental2.KmOut);
   }
}
