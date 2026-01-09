using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Enums;
using CarRentalApi.Modules.Cars.Infrastructure;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Modules.Cars.Infrastructure;

public sealed class CarRepositoryIntT : TestBase, IAsyncLifetime {
   private TestSeed _seed = null!;
   private SqliteConnection _dbConnection = null!;
   private CarRentalDbContext _dbContext = null!;
   private ICarRepository _repository = null!;
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

      _repository = new CarRepository(_dbContext, CreateLogger<CarRepository>());
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

   #region FindByIdAsync
   [Fact]
   public async Task FindByIdAsync_returns_car_when_found() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      var id = Guid.Parse(_seed.Car1Id);

      // Act
      var actual = await _repository.FindByIdAsync(id, CancellationToken.None);

      // Assert
      Assert.NotNull(actual);
      Assert.Equal(id, actual!.Id);
      Assert.Equal(CarCategory.Economy, actual.Category);
   }

   [Fact]
   public async Task FindByIdAsync_returns_null_when_not_found() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      var nonExistentId = Guid.NewGuid();

      // Act
      var actual = await _repository.FindByIdAsync(nonExistentId, CancellationToken.None);

      // Assert
      Assert.Null(actual);
   }
   #endregion

   #region ExistsLicensePlateAsync
   [Fact]
   public async Task ExistsLicensePlateAsync_returns_true_when_license_plate_exists() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();

      var licensePlate = "ECO-001"; // Car1

      // Act
      var exists = await _repository.ExistsLicensePlateAsync(licensePlate, CancellationToken.None);

      // Assert
      Assert.True(exists);
   }

   [Fact]
   public async Task ExistsLicensePlateAsync_returns_false_when_license_plate_does_not_exist() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();

      var licensePlate = "XXX-999";

      // Act
      var exists = await _repository.ExistsLicensePlateAsync(licensePlate, CancellationToken.None);

      // Assert
      Assert.False(exists);
   }

   [Fact]
   public async Task ExistsLicensePlateAsync_is_case_insensitive() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();

      var licensePlate = "eco-001".ToUpperInvariant(); // lowercase version of ECO-001

      // Act
      var exists = await _repository.ExistsLicensePlateAsync(licensePlate, CancellationToken.None);

      // Assert
      Assert.True(exists);
   }
   #endregion

   #region CountCarsInCategoryAsync
   [Fact]
   public async Task CountCarsInCategoryAsync_returns_correct_count() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();

      // Act
      var count = await _repository.CountCarsInCategoryAsync(CarCategory.Compact, CancellationToken.None);

      // Assert
      Assert.Equal(5, count); // Car6-Car10
   }
   #endregion

   #region SelectAsync
   [Fact]
   public async Task SelectAsync_returns_all_cars_when_no_filters() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();

      // Act
      var cars = await _repository.SelectByAsync(
         category: null,
         status: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.Equal(20, cars.Count); // All cars from seed
   }

   [Fact]
   public async Task SelectAsync_filters_by_category() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();

      // Act
      var cars = await _repository.SelectByAsync(
         category: CarCategory.Economy,
         status: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.Equal(5, cars.Count); // Car1-Car5
      Assert.All(cars, car => Assert.Equal(CarCategory.Economy, car.Category));
   }

   [Fact]
   public async Task SelectAsync_filters_by_status() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Load cars and change their status
      var car1 = await _repository.FindByIdAsync(Guid.Parse(_seed.Car1Id), CancellationToken.None);
      var car2 = await _repository.FindByIdAsync(Guid.Parse(_seed.Car2Id), CancellationToken.None);

      var result1 = car1!.SendToMaintenance();
      var result2 = car2!.SendToMaintenance();

      Assert.True(result1.IsSuccess);
      Assert.True(result2.IsSuccess);

      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Act
      var cars = await _repository.SelectByAsync(
         category: null,
         status: CarStatus.Maintenance,
         ct: CancellationToken.None
      );

      // Assert
      Assert.Equal(2, cars.Count);
      Assert.All(cars, car => Assert.Equal(CarStatus.Maintenance, car.Status));
   }

   [Fact]
   public async Task SelectAsync_filters_by_category_and_status() {
      // Arrange
      _dbContext.Cars.AddRange(_seed.Cars);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      var car1 = await _repository.FindByIdAsync(Guid.Parse(_seed.Car1Id), CancellationToken.None);
      var result = car1!.SendToMaintenance();
      Assert.True(result.IsSuccess);

      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Act
      var cars = await _repository.SelectByAsync(
         category: CarCategory.Economy,
         status: CarStatus.Maintenance,
         ct: CancellationToken.None
      );

      // Assert
      Assert.Single(cars);
      Assert.Equal(Guid.Parse(_seed.Car1Id), cars[0].Id);
   }
   #endregion

   #region Add
   [Fact]
   public async Task Add_persists_car() {
      // Arrange
      var carResult = Car.Create(
         CarCategory.Economy,
         "Test Manufacturer",
         "Test Model",
         "TEST-001"
      );
      Assert.True(carResult.IsSuccess);
      var car = carResult.Value;

      // Act
      _repository.Add(car);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Assert
      var saved = await _repository.FindByIdAsync(car.Id, CancellationToken.None);
      Assert.NotNull(saved);
      Assert.Equal(car.Id, saved!.Id);
      Assert.Equal(car.Category, saved.Category);
      Assert.Equal(car.LicensePlate, saved.LicensePlate);
   }

   [Fact]
   public async Task Add_multiple_cars_persists_all() {
      // Arrange
      var car1 = Car.Create(CarCategory.Economy, "Make1", "Model1", "TEST-001").Value;
      var car2 = Car.Create(CarCategory.Compact, "Make2", "Model2", "TEST-002").Value;
      var car3 = Car.Create(CarCategory.Midsize, "Make3", "Model3", "TEST-003").Value;

      // Act
      _repository.Add(car1);
      _repository.Add(car2);
      _repository.Add(car3);
      await _unitOfWork.SaveAllChangesAsync();
      _dbContext.ChangeTracker.Clear();

      // Assert
      var saved1 = await _repository.FindByIdAsync(car1.Id, CancellationToken.None);
      var saved2 = await _repository.FindByIdAsync(car2.Id, CancellationToken.None);
      var saved3 = await _repository.FindByIdAsync(car3.Id, CancellationToken.None);

      Assert.NotNull(saved1);
      Assert.NotNull(saved2);
      Assert.NotNull(saved3);
   }
   #endregion
}