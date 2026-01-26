using CarRentalApi._2_Modules.Cars._1_Ports.Outbound;
using CarRentalApi._2_Modules.Cars._2_Application.UseCases;
using CarRentalApi._2_Modules.Cars._3_Domain.Errors;
using CarRentalApi._2_Modules.Cars._4_Infrastructure.Repositories;
using CarRentalApi._3_Infrastructure.Persistence.Database;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Modules.Cars.Application.UseCases;

public sealed class CarUcCreateIt : TestBase, IAsyncLifetime {
   private SqliteConnection _dbConnection = null!;
   private CarRentalDbContext _dbContext = null!;
   private ICarRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private CarUcCreate _sut = null!;
   private TestSeed _seed = null!;

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

      _repository = new CarRepositoryEf(_dbContext, CreateLogger<CarRepositoryEf>());
      _unitOfWork = new UnitOfWork(_dbContext, CreateLogger<UnitOfWork>());

      // Seed cars from TestSeed
      _repository.Add(_seed.Car1);
      _repository.Add(_seed.Car2);
      _repository.Add(_seed.Car3);
      await _unitOfWork.SaveAllChangesAsync("Seed cars", CancellationToken.None);

      _sut = new CarUcCreate(_repository, _unitOfWork, CreateLogger<CarUcCreate>());
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
   public async Task ExecuteAsync_WithValidData_ShouldPersistCar() {
      // Arrange
      var id = _seed.Car10.Id;
      var manufacturer = _seed.Car10.Manufacturer;
      var model = _seed.Car10.Model;
      var licensePlate = _seed.Car10.LicensePlate;
      var category = _seed.Car10.Category;
      var status = _seed.Car10.Status;
      var createdAt = _seed.Car10.CreatedAt;

      // Act
      var result = await _sut.ExecuteAsync(manufacturer, model, licensePlate,
         category, createdAt, id.ToString(), CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);
      var CarId = result.Value;
      Assert.NotEqual(Guid.Empty, CarId);

      var actual = await _repository.FindByIdAsync(CarId, CancellationToken.None);
      Assert.NotNull(actual);
      Assert.Equal(manufacturer, actual.Manufacturer);
      Assert.Equal(model, actual.Model);
      Assert.Equal(licensePlate, actual!.LicensePlate);
      Assert.Equal(category, actual.Category);
      Assert.Equal(status, actual.Status);
   }

   [Fact]
   public async Task ExecuteAsync_WithDuplicateLicensePlate_ShouldFail() {
      // Arrange - Car1 is already seeded
      var id = _seed.Car1.Id;
      var manufacturer = _seed.Car1.Manufacturer;
      var model = _seed.Car1.Model;
      var licensePlate = _seed.Car1.LicensePlate;
      var category = _seed.Car10.Category;
      var status = _seed.Car10.Status;
      var createdAt = _seed.Car10.CreatedAt;

      // Act - Use existing Car1 license plate
      var result = await _sut.ExecuteAsync(
         manufacturer,
         model,
         licensePlate,
         category,
         createdAt,
         id.ToString(),
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsFailure);
   }

   [Theory]
   [InlineData("", "VW", "Golf")]
   [InlineData("VW", "", "Golf")]
   [InlineData("VW", "Golf", "")]
   public async Task ExecuteAsync_WithMissingRequiredData_ShouldFail(
      string manufacturer,
      string model,
      string licensePlate
   ) {
      // Act
      var result = await _sut.ExecuteAsync(
         manufacturer,
         model,
         licensePlate,
         CarCategory.Compact,
         _seed.FixedNow,
         id: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsFailure);
   }

   [Theory]
   [InlineData("   ", "VW", "Golf")]
   [InlineData("VW", "   ", "Golf")]
   [InlineData("VW", "Golf", "   ")]
   public async Task ExecuteAsync_WithWhitespaceData_ShouldFail(
      string make,
      string model,
      string licensePlate
   ) {
      // Act
      var result = await _sut.ExecuteAsync(
         make,
         model,
         licensePlate,
         CarCategory.Compact,
         _seed.FixedNow,
         id: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsFailure);
   }

   [Fact]
   public async Task ExecuteAsync_WithInvalidId_ShouldFail() {
      // Act
      var result = await _sut.ExecuteAsync(
         _seed.Car1.Manufacturer,
         _seed.Car1.Model,
         _seed.Car1.LicensePlate,
         CarCategory.Compact,
         _seed.FixedNow,
         id: "not-a-valid-guid",
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.InvalidId.Code, result.Error.Code);
   }
}