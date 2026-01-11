using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Cars.Application.UseCases;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Enums;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Infrastructure;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using CarRentalApi.Modules.Cars.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Modules.Cars.Application.UseCases;

public sealed class CarUcSendToMaintenanceIntT : TestBase, IAsyncLifetime {
   private SqliteConnection _dbConnection = null!;
   private CarRentalDbContext _dbContext = null!;
   private ICarRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private CarUcSendToMaintenance _uc = null!;

   private Car _availableCar = null!;
   private Car _retiredCar = null!;

   public async Task InitializeAsync() {
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

      _uc = new CarUcSendToMaintenance(
         _repository,
         _unitOfWork,
         CreateLogger<CarUcSendToMaintenance>()
      );

      // Seed: one available car
      _availableCar = Car.Create(
         category: CarCategory.Compact,
         manufacturer: "VW",
         model: "Golf",
         licensePlate: "BS-CR-1001",
         id: "00000000-0100-0000-0000-000000000000"
      ).Value!;

      // Seed: one retired car
      _retiredCar = Car.Create(
         category: CarCategory.Compact,
         manufacturer: "VW",
         model: "Passat",
         licensePlate: "BS-CR-9999",
         id: "00000000-0999-0000-0000-000000000000"
      ).Value!;
      Assert.True(_retiredCar.Retire().IsSuccess);

      _dbContext.Cars.AddRange(_availableCar, _retiredCar);
      await _unitOfWork.SaveAllChangesAsync("seed cars", CancellationToken.None);
      _unitOfWork.ClearChangeTracker();
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
   public async Task ExecuteAsync_sets_status_from_available_to_maintenance_and_persists() {
      // Arrange
      var id = _availableCar.Id;

      // Act
      var result = await _uc.ExecuteAsync(id, CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);

      _unitOfWork.ClearChangeTracker();
      var reloaded = await _repository.FindByIdAsync(id, CancellationToken.None);

      Assert.NotNull(reloaded);
      Assert.Equal(CarStatus.Maintenance, reloaded!.Status);
   }

   [Fact]
   public async Task ExecuteAsync_rejects_retired_car() {
      // Arrange
      var id = _retiredCar.Id;

      // Act
      var result = await _uc.ExecuteAsync(id, CancellationToken.None);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.InvalidStatusTransition.Code, result.Error.Code);

      _unitOfWork.ClearChangeTracker();
      var reloaded = await _repository.FindByIdAsync(id, CancellationToken.None);
      Assert.NotNull(reloaded);
      Assert.Equal(CarStatus.Retired, reloaded!.Status);
   }

   [Fact]
   public async Task ExecuteAsync_returns_not_found_when_car_missing() {
      // Arrange
      var missingId = Guid.Parse("00000000-0000-0000-0000-000000000404");

      // Act
      var result = await _uc.ExecuteAsync(missingId, CancellationToken.None);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.NotFound.Code, result.Error.Code);
   }
}