using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Utils;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Enums;
using CarRentalApi.Modules.Cars.Domain.Errors;

namespace CarRentalApiTest.Domain.Entities;

public sealed class CarUt {
   // Creation via TestSeed
   [Fact]
   public void Car_from_TestSeed_is_valid() {
      // Arrange
      var seed = new TestSeed();

      // Act
      var car = seed.Car1;

      // Assert
      Assert.NotNull(car);
      Assert.Equal(seed.Car1Id.ToGuid(), car.Id);
      Assert.Equal(CarCategory.Economy, car.Category);
      Assert.Equal("VW", car.Manufacturer);
      Assert.Equal("Polo", car.Model);
      Assert.Equal("ECO-001", car.LicensePlate);
      Assert.Equal(CarStatus.Available, car.Status);
   }

   // ------------------------------------------------------------------
   // Car.Create() - valid cases
   // ------------------------------------------------------------------

   [Fact]
   public void Create_returns_valid_car_with_correct_properties() {
      // Arrange & Act
      var result = Car.Create(
         CarCategory.Compact,
         "BMW",
         "3 Series",
         "COM-123"
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.NotNull(result.Value);
      Assert.Equal(CarCategory.Compact, result.Value.Category);
      Assert.Equal("BMW", result.Value.Manufacturer);
      Assert.Equal("3 Series", result.Value.Model);
      Assert.Equal("COM-123", result.Value.LicensePlate);
      Assert.Equal(CarStatus.Available, result.Value.Status);
   }

   // ------------------------------------------------------------------
   // LicensePlate validation
   // ------------------------------------------------------------------

   [Fact]
   public void Create_rejects_empty_license_plate() {
      // Arrange & Act
      var result = Car.Create(
         CarCategory.Economy,
         "VW",
         "Polo",
         ""
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.LicensePlateIsRequired.Code, result.Error!.Code);
   }

   [Fact]
   public void Create_rejects_whitespace_license_plate() {
      // Arrange & Act
      var result = Car.Create(
         CarCategory.Economy,
         "VW",
         "Polo",
         "   "
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.LicensePlateIsRequired.Code, result.Error!.Code);
   }

   [Fact]
   public void Create_rejects_invalid_license_plate_format() {
      // Arrange & Act
      var result = Car.Create(
         CarCategory.Economy,
         "VW",
         "Polo",
         "eco-001" // lowercase not allowed
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.InvalidLicensePlateFormat.Code, result.Error!.Code);
   }

   [Theory]
   [InlineData("ECO-001")]
   [InlineData("M-AB-1234")]
   [InlineData("B-XX-999")]
   [InlineData("ABC-12")]
   [InlineData("A-1")]
   [InlineData("XXX-9999")]
   public void Create_accepts_various_valid_license_plate_formats(string licensePlate) {
      // Arrange & Act
      var result = Car.Create(
         CarCategory.Economy,
         "VW",
         "Polo",
         licensePlate
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(licensePlate, result.Value.LicensePlate);
   }

   [Theory]
   [InlineData("eco-001")]      // lowercase
   [InlineData("ECO_001")]      // underscore
   [InlineData("ECO 001")]      // space
   [InlineData("ECO.001")]      // dot
   [InlineData("ÄCO-001")]      // umlaut
   public void Create_rejects_invalid_license_plate_formats(string licensePlate) {
      // Arrange & Act
      var result = Car.Create(
         CarCategory.Economy,
         "VW",
         "Polo",
         licensePlate
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.InvalidLicensePlateFormat.Code, result.Error!.Code);
   }

   // ------------------------------------------------------------------
   // CarStatus machine - valid transitions
   // ------------------------------------------------------------------

   [Fact]
   public void MarkAsRented_changes_status_from_Available_to_Rented() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1; // Available

      // Act
      var result = car.MarkAsRented();

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(CarStatus.Rented, car.Status);
   }

   [Fact]
   public void MarkAsAvailable_changes_status_from_Rented_to_Available() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1;
      Assert.True(car.MarkAsRented().IsSuccess); // now Rented

      // Act
      var result = car.MarkAsAvailable();

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(CarStatus.Available, car.Status);
   }

   [Fact]
   public void SendToMaintenance_changes_status_from_Available_to_Maintenance() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1; // Available

      // Act
      var result = car.SendToMaintenance();

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(CarStatus.Maintenance, car.Status);
   }

   [Fact]
   public void ReturnFromMaintenance_changes_status_from_Maintenance_to_Available() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1;
      Assert.True(car.SendToMaintenance().IsSuccess); // now Maintenance

      // Act
      var result = car.ReturnFromMaintenance();

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(CarStatus.Available, car.Status);
   }

   // ------------------------------------------------------------------
   // CarStatus machine - invalid transitions
   // ------------------------------------------------------------------

   [Fact]
   public void MarkAsRented_rejects_when_not_Available() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1;
      Assert.True(car.SendToMaintenance().IsSuccess); // now Maintenance

      // Act
      var result = car.MarkAsRented();

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.CarNotAvailable.Code, result.Error!.Code);
      Assert.Equal(CarStatus.Maintenance, car.Status);
   }

   [Fact]
   public void MarkAsAvailable_rejects_when_not_Rented() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1; // Available

      // Act
      var result = car.MarkAsAvailable();

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.InvalidStatusTransition.Code, result.Error!.Code);
      Assert.Equal(CarStatus.Available, car.Status);
   }

   [Fact]
   public void SendToMaintenance_rejects_when_not_Available() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1;
      Assert.True(car.MarkAsRented().IsSuccess); // now Rented

      // Act
      var result = car.SendToMaintenance();

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.InvalidStatusTransition.Code, result.Error!.Code);
      Assert.Equal(CarStatus.Rented, car.Status);
   }

   [Fact]
   public void ReturnFromMaintenance_rejects_when_not_Maintenance() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1; // Available

      // Act
      var result = car.ReturnFromMaintenance();

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.InvalidStatusTransition.Code, result.Error!.Code);
      Assert.Equal(CarStatus.Available, car.Status);
   }

   // ------------------------------------------------------------------
   // Retire (User Story 1.4)
   // ------------------------------------------------------------------

   [Fact]
   public void Retire_sets_status_to_Retired_from_any_non_retired_state() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1;

      // Move into a different state first (Maintenance)
      Assert.True(car.SendToMaintenance().IsSuccess);
      Assert.Equal(CarStatus.Maintenance, car.Status);

      // Act
      var result = car.Retire();

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(CarStatus.Retired, car.Status);
   }

   [Fact]
   public void Retire_is_idempotent() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1;

      Assert.True(car.Retire().IsSuccess);
      Assert.Equal(CarStatus.Retired, car.Status);

      // Act
      var result = car.Retire();

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(CarStatus.Retired, car.Status);
   }

   [Fact]
   public void Retired_car_cannot_change_status_anymore() {
      // Arrange
      var seed = new TestSeed();
      var car = seed.Car1;

      Assert.True(car.Retire().IsSuccess);
      Assert.Equal(CarStatus.Retired, car.Status);

      // Act
      var r1 = car.MarkAsRented();
      var r2 = car.SendToMaintenance();
      var r3 = car.ReturnFromMaintenance();
      var r4 = car.MarkAsAvailable();

      // Assert
      Assert.True(r1.IsFailure);
      Assert.True(r2.IsFailure);
      Assert.True(r3.IsFailure);
      Assert.True(r4.IsFailure);

      Assert.Equal(CarErrors.InvalidStatusTransition.Code, r1.Error!.Code);
      Assert.Equal(CarErrors.InvalidStatusTransition.Code, r2.Error!.Code);
      Assert.Equal(CarErrors.InvalidStatusTransition.Code, r3.Error!.Code);
      Assert.Equal(CarErrors.InvalidStatusTransition.Code, r4.Error!.Code);

      Assert.Equal(CarStatus.Retired, car.Status);
   }

   // ------------------------------------------------------------------
   // Entity equality (identity-based)
   // ------------------------------------------------------------------

   [Fact]
   public void Cars_with_same_Id_are_equal_even_if_properties_differ() {
      // Arrange
      var seed1 = new TestSeed();
      var seed2 = new TestSeed();

      var carA = seed1.Car1;
      var carB = seed2.Car1;

      // sanity: different references
      Assert.NotSame(carA, carB);

      // Act + Assert (DDD identity)
      Assert.True(carA.Equals(carB));
      Assert.Equal(carA.GetHashCode(), carB.GetHashCode());
   }

   [Fact]
   public void Cars_with_different_Id_are_not_equal() {
      // Arrange
      var seed = new TestSeed();
      var carA = seed.Car1;
      var carB = seed.Car2;

      // Act + Assert
      Assert.False(carA.Equals(carB));
   }

   [Fact]
   public void Equality_operator_compares_identity_if_overloaded() {
      // Arrange
      var seed1 = new TestSeed();
      var seed2 = new TestSeed();

      var a = seed1.Car1;
      var b = seed2.Car1;

      // Assert
      Assert.True(a == b);
      Assert.False(a != b);
   }
}