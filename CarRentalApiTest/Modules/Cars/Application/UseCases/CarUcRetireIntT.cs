using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Cars.Application.UseCases;
using CarRentalApi.Modules.Cars.Domain.Enums;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Domain.Policies;
using CarRentalApi.Modules.Cars.Infrastructure;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Modules.Cars.Application.UseCases;

public sealed class CarUcRetireIt : TestBase, IAsyncLifetime {
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

      // Persist seed cars
      _dbContext.Cars.AddRange(_seed.Cars);
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
   public async Task ExecuteAsync_retires_car_when_policy_allows_and_persists() {
      // Arrange
      var uc = new CarUcRetire(
         _repository,
         _unitOfWork,
         new AllowAllCarRemovalPolicy(),
         CreateLogger<CarUcRetire>()
      );

      var id = Guid.Parse(_seed.Car6Id); // pick one seed car (Compact COM-001)

      // Act
      var result = await uc.ExecuteAsync(id, CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);

      _unitOfWork.ClearChangeTracker();
      var reloaded = await _repository.FindByIdAsync(id, CancellationToken.None);

      Assert.NotNull(reloaded);
      Assert.Equal(CarStatus.Retired, reloaded!.Status);
   }

   [Fact]
   public async Task ExecuteAsync_rejects_retire_when_policy_denies_and_does_not_change_status() {
      // Arrange
      var uc = new CarUcRetire(
         _repository,
         _unitOfWork,
         new DenyCarRemovalPolicy(),
         CreateLogger<CarUcRetire>()
      );

      var id = Guid.Parse(_seed.Car6Id);

      // Act
      var result = await uc.ExecuteAsync(id, CancellationToken.None);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.InvalidStatusTransition.Code, result.Error.Code);

      _unitOfWork.ClearChangeTracker();
      var reloaded = await _repository.FindByIdAsync(id, CancellationToken.None);

      Assert.NotNull(reloaded);
      Assert.Equal(CarStatus.Available, reloaded!.Status);
   }

   [Fact]
   public async Task ExecuteAsync_returns_not_found_when_car_missing() {
      // Arrange
      var uc = new CarUcRetire(
         _repository,
         _unitOfWork,
         new AllowAllCarRemovalPolicy(),
         CreateLogger<CarUcRetire>()
      );

      var missingId = Guid.Parse("00000000-0000-0000-0000-000000000404");

      // Act
      var result = await uc.ExecuteAsync(missingId, CancellationToken.None);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CarErrors.NotFound.Code, result.Error.Code);
   }

   [Fact]
   public async Task After_retire_car_cannot_be_reactivated_status_stays_retired() {
      // Arrange
      var uc = new CarUcRetire(
         _repository,
         _unitOfWork,
         new AllowAllCarRemovalPolicy(),
         CreateLogger<CarUcRetire>()
      );

      var id = Guid.Parse(_seed.Car6Id);

      // Act 1: retire via use case (persists)
      var retireResult = await uc.ExecuteAsync(id, CancellationToken.None);

      // Assert 1
      Assert.True(retireResult.IsSuccess);

      _unitOfWork.ClearChangeTracker();
      var retiredCar = await _repository.FindByIdAsync(id, CancellationToken.None);

      Assert.NotNull(retiredCar);
      Assert.Equal(CarStatus.Retired, retiredCar!.Status);

      // Act 2: try to reactivate (domain operation must be blocked)
      // Note: ReturnFromMaintenance requires Maintenance -> Available,
      // but the *strong invariant* is: Retired cannot change status at all.
      // So it must fail regardless of the "from" state.
      var reactivateResult = retiredCar.ReturnFromMaintenance();

      // Assert 2
      Assert.True(reactivateResult.IsFailure);
      Assert.Equal(CarErrors.InvalidStatusTransition.Code, reactivateResult.Error.Code);
      Assert.Equal(CarStatus.Retired, retiredCar.Status);

      // Persist attempt (should not change anything anyway)
      await _unitOfWork.SaveAllChangesAsync("attempt reactivate after retire", CancellationToken.None);

      _unitOfWork.ClearChangeTracker();
      var reloaded = await _repository.FindByIdAsync(id, CancellationToken.None);

      Assert.NotNull(reloaded);
      Assert.Equal(CarStatus.Retired, reloaded!.Status);
   }

   [Fact]
   public async Task Lifecycle_available_to_maintenance_to_retired_then_reactivate_fails_status_stays_retired() {
      // Arrange
      var retireUc = new CarUcRetire(
         _repository,
         _unitOfWork,
         new AllowAllCarRemovalPolicy(),
         CreateLogger<CarUcRetire>()
      );

      var id = Guid.Parse(_seed.Car6Id); // pick a seed car that starts Available

      // Step 1: Available -> Maintenance (User Story 1.2) via domain operation
      var car = await _repository.FindByIdAsync(id, CancellationToken.None);
      Assert.NotNull(car);
      Assert.Equal(CarStatus.Available, car!.Status);

      var toMaintenance = car.SendToMaintenance();
      Assert.True(toMaintenance.IsSuccess);

      await _unitOfWork.SaveAllChangesAsync("set maintenance", CancellationToken.None);
      _unitOfWork.ClearChangeTracker();

      var maintenanceCar = await _repository.FindByIdAsync(id, CancellationToken.None);
      Assert.NotNull(maintenanceCar);
      Assert.Equal(CarStatus.Maintenance, maintenanceCar!.Status);

      // Step 2: Maintenance -> Retired (User Story 1.4) via use case
      var retireResult = await retireUc.ExecuteAsync(id, CancellationToken.None);
      Assert.True(retireResult.IsSuccess);

      _unitOfWork.ClearChangeTracker();
      var retiredCar = await _repository.FindByIdAsync(id, CancellationToken.None);
      Assert.NotNull(retiredCar);
      Assert.Equal(CarStatus.Retired, retiredCar!.Status);

      // Step 3: Try to reactivate (User Story 1.3) -> must fail because Retired is terminal
      var reactivate = retiredCar.ReturnFromMaintenance();
      Assert.True(reactivate.IsFailure);
      Assert.Equal(CarErrors.InvalidStatusTransition.Code, reactivate.Error.Code);
      Assert.Equal(CarStatus.Retired, retiredCar.Status);

      // Persist attempt + reload (should still be Retired)
      await _unitOfWork.SaveAllChangesAsync("attempt reactivate after retire", CancellationToken.None);
      _unitOfWork.ClearChangeTracker();

      var reloaded = await _repository.FindByIdAsync(id, CancellationToken.None);
      Assert.NotNull(reloaded);
      Assert.Equal(CarStatus.Retired, reloaded!.Status);
   }

   // ------------------------------------------------------------
   // Policies used in this IT
   // ------------------------------------------------------------
   private sealed class AllowAllCarRemovalPolicy : ICarRemovalPolicy {
      public Task<bool> CheckAsync(Guid carId, CancellationToken ct) =>
         Task.FromResult(true);
   }

   private sealed class DenyCarRemovalPolicy : ICarRemovalPolicy {
      public Task<bool> CheckAsync(Guid carId, CancellationToken ct) =>
         Task.FromResult(false);
   }
}