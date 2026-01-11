using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Cars.Application.UseCases;
using CarRentalApi.Modules.Cars.Domain.Enums;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Infrastructure;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using CarRentalApi.Modules.Cars.Ports.Outbound;
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
      _seed = new  TestSeed(); 
      
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
      // Act
      var result = await _sut.ExecuteAsync(
         CarCategory.Midsize,
         "BMW",
         "3 Series",
         "MID-001",
         id: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.NotEqual(Guid.Empty, result.Value.Id);

      var fromDb = await _repository.FindByIdAsync(result.Value.Id, CancellationToken.None);
      Assert.NotNull(fromDb);
      Assert.Equal("MID-001", fromDb!.LicensePlate);
      Assert.Equal("BMW", fromDb.Manufacturer);
      Assert.Equal("3 Series", fromDb.Model);
      Assert.Equal(CarCategory.Midsize, fromDb.Category);
      Assert.Equal(CarStatus.Available, fromDb.Status);
   }

   [Fact]
   public async Task ExecuteAsync_WithDuplicateLicensePlate_ShouldFail() {
      // Act - Use existing Car1 license plate
      var result = await _sut.ExecuteAsync(
         CarCategory.Economy,
         "Different",
         "Model",
         _seed.Car1.LicensePlate,
         id: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.LicensePlateMustBeUnique.Code, result.Error.Code);
   }

   [Theory]
   [InlineData("", "VW", "Golf")]
   [InlineData("VW", "", "Golf")]
   [InlineData("VW", "Golf", "")]
   public async Task ExecuteAsync_WithMissingRequiredData_ShouldFail(
      string make,
      string model,
      string licensePlate
   ) {
      // Act
      var result = await _sut.ExecuteAsync(
         CarCategory.Compact,
         make,
         model,
         licensePlate,
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
         CarCategory.Compact,
         make,
         model,
         licensePlate,
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
         CarCategory.Compact,
         "VW",
         "Golf",
         "INVALID-ID",
         id: "not-a-valid-guid",
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.InvalidId.Code, result.Error.Code);
   }

   [Fact]
   public async Task ExecuteAsync_WithNullId_ShouldGenerateNewId() {
      // Act
      var result = await _sut.ExecuteAsync(
         CarCategory.Compact,
         "VW",
         "Golf",
         "AUTO-ID-001",
         id: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.NotEqual(Guid.Empty, result.Value.Id);
   }

   [Fact]
   public async Task ExecuteAsync_WithValidGuidId_ShouldUseProvidedId() {
      // Arrange
      var customId = "99990000-0000-0000-0000-000000000000";

      // Act
      var result = await _sut.ExecuteAsync(
         CarCategory.Suv,
         "Audi",
         "Q5",
         "SUV-001",
         id: customId,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(Guid.Parse(customId), result.Value.Id);
   }

   [Fact]
   public async Task ExecuteAsync_WithDuplicateId_ShouldFail() {
      // Act - Use existing Car1 ID
      var result = await _sut.ExecuteAsync(
         CarCategory.Economy,
         "Different",
         "Model",
         "DUPLICATE-001",
         id: _seed.Car1Id,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsFailure);
   }

   [Fact]
   public async Task ExecuteAsync_WithCancelledToken_ShouldThrow() {
      // Arrange
      var cts = new CancellationTokenSource();
      cts.Cancel();

      // Act & Assert
      await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
         await _sut.ExecuteAsync(
            CarCategory.Compact,
            "VW",
            "Golf",
            "CANCEL-001",
            id: null,
            ct: cts.Token
         )
      );
   }

   [Fact]
   public async Task ExecuteAsync_MultipleCars_ShouldPersistAll() {
      // Arrange & Act - Create cars in different categories
      var result1 = await _sut.ExecuteAsync(
         CarCategory.Compact,
         "VW",
         "Golf",
         "MULTI-101",
         id: null,
         ct: CancellationToken.None
      );

      var result2 = await _sut.ExecuteAsync(
         CarCategory.Suv,
         "Audi",
         "Q5",
         "MULTI-102",
         id: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result1.IsSuccess);
      Assert.True(result2.IsSuccess);

      // Test SelectAsync with different parameters
      var allCars = await _repository.SelectByAsync(null, null, CancellationToken.None);
      Assert.True(allCars.Count >= 5); // 3 Seed + 2 new

      var compactCars = await _repository.SelectByAsync(CarCategory.Compact, null, CancellationToken.None);
      Assert.True(compactCars.Count >= 1); // At least MULTI-101

      var availableCars = await _repository.SelectByAsync(null, CarStatus.Available, CancellationToken.None);
      Assert.True(availableCars.Count >= 5); // All created cars should be available

      var compactAvailable = await _repository.SelectByAsync(
         CarCategory.Compact,
         CarStatus.Available,
         CancellationToken.None
      );
      Assert.True(compactAvailable.Count >= 1); // MULTI-101
   }

   [Theory]
   [InlineData(CarCategory.Economy)]
   [InlineData(CarCategory.Compact)]
   [InlineData(CarCategory.Midsize)]
   [InlineData(CarCategory.Suv)]
   public async Task ExecuteAsync_WithAllValidCategories_ShouldSucceed(CarCategory category) {
      // Arrange
      var licensePlate = $"CAT-{(int)category:D3}";

      // Act
      var result = await _sut.ExecuteAsync(
         category,
         "Test",
         "Model",
         licensePlate,
         id: null,
         ct: CancellationToken.None
      );

      // Assert
      Assert.True(result.IsSuccess);

      var fromDb = await _repository.FindByIdAsync(result.Value.Id, CancellationToken.None);
      Assert.NotNull(fromDb);
      Assert.Equal(category, fromDb!.Category);

      // Verify the car appears in category-filtered queries
      var categoryCars = await _repository.SelectByAsync(category, null, CancellationToken.None);
      Assert.Contains(categoryCars, c => c.Id == result.Value.Id);
   }
}
