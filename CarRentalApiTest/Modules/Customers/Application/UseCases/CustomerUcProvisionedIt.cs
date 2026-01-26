using CarRentalApi._2_Modules.Customers._1_Ports.Outbound;
using CarRentalApi._2_Modules.Customers._2_Application.UseCases;
using CarRentalApi._3_Infrastructure.Persistence.Database;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Modules.Customers.Application.UseCases;

public sealed class CustomerProvisinedIt : TestBase, IAsyncLifetime {
   private SqliteConnection _dbConnection = null!;
   private CarRentalDbContext _dbContext = null!;
   private IIdentityGateway _identityGateway = null!;
   private ICustomerRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private CustomerUcProvisioned _sut = null!;
   private TestSeed _seed = null!;
   private string _dbPath = null!;

   public async Task InitializeAsync() {
      _seed = new TestSeed();

      // In-memory SQLite
      // _dbConnection = new SqliteConnection("Filename=:memory:");
      
      // SQLite file-based
      var testDbDir = Path.Combine(AppContext.BaseDirectory, "TestDbs");
      Directory.CreateDirectory(testDbDir);
     // var dbFileName = $"customers-it-{Guid.NewGuid():N}.db";
      var dbFileName = $"customers-it.db";
      _dbPath = Path.Combine(testDbDir, dbFileName);
      
      // 1) Delete DB file (must happen before opening any connection)
      if (File.Exists(_dbPath))
         File.Delete(_dbPath);

      // 2) Recreate via migrations
      _dbConnection = new SqliteConnection($"Data Source={_dbPath}");
      
      // 3) Open connection
      await _dbConnection.OpenAsync();

      // 4) Create DbContext
      var options = new DbContextOptionsBuilder<CarRentalDbContext>()
         .UseSqlite(_dbConnection)
         .EnableSensitiveDataLogging()
         .Options;
      _dbContext = new CarRentalDbContext(options);
      // IMPORTANT: runs EF Core migrations (instead of EnsureCreated)
      await _dbContext.Database.MigrateAsync();
      
      _repository = new CustomerRepositoryEf(_dbContext);
      _unitOfWork = new UnitOfWork(_dbContext, CreateLogger<UnitOfWork>());

      // Seed cars from TestSeed
      _repository.Add(_seed.Customer1);
      _repository.Add(_seed.Customer2);
      _repository.Add(_seed.Customer3);
      await _unitOfWork.SaveAllChangesAsync("Seed cars", CancellationToken.None);

      // Default gateway for success tests: subject of Customer5, not an employee/admin
      _identityGateway = new FakeIdentityGateway(
         subject: _seed.Customer5.Subject.Value,
         username: _seed.Customer5.Email.Value,
         createdAt: _seed.Customer5.CreatedAt,
         adminRights: 0
      );
      
      // System under test
      _sut = new CustomerUcProvisioned(
         _identityGateway,
         _repository,
         _unitOfWork,
         CreateLogger<CustomerUcProvisioned>()
      );
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
      // Optional: delete at end as well (keeps workspace clean)
      if (!string.IsNullOrWhiteSpace(_dbPath) && File.Exists(_dbPath))
         File.Delete(_dbPath);
   }

   [Fact]
   public async Task ExecuteAsync_WithValidData_ShouldPersistCustomer() {
      // Arrange
      _identityGateway = new FakeIdentityGateway(
         subject: _seed.Customer5.Subject.Value,
         username: _seed.Customer5.Email.Value,
         createdAt: _seed.Customer5.CreatedAt,
         adminRights: 0
      );
      
      
      // Act
      var result = await _sut.ExecuteAsync(CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);
      var CustomerId = result.Value;
      Assert.NotEqual(Guid.Empty, CustomerId);

      var actual = await _repository.FindByIdAsync(CustomerId, CancellationToken.None);
      Assert.NotNull(actual);

      // Assert.Equal(firstname, actual.Firstname);
      // Assert.Equal(lastname, actual.Lastname);
      // Assert.Equal(email, actual.Email);
      // Assert.Equal(identitySubject, actual.Subject);
   }
}