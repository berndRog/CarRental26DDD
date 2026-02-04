using CarRentalApi._2_Modules.Customers._1_Ports.Outbound;
using CarRentalApi._2_Modules.Customers._2_Application.UseCases;
using CarRentalApi._2_Modules.Employees._3_Domain.Enums;
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
   private CustomerUcProvision _sut = null!;
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
         subject: _seed.Customer5.Subject,
         username: _seed.Customer5.Email,
         createdAt: _seed.Customer5.CreatedAt,
         adminRights: 0
      );
      
      // System under test
      _sut = new CustomerUcProvision(
         _identityGateway,
         _repository,
         _unitOfWork,
         CreateLogger<CustomerUcProvision>()
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
   public async Task ExecuteAsync_WithValidData_ShouldProvisonCustomer() {
      // Arrange
      var id = _seed.Customer5.Id.ToString();
      var subject = _seed.Customer5.Subject;
      var username = _seed.Customer5.Email;
      var createdAt = _seed.Customer5.CreatedAt;
      _identityGateway = new FakeIdentityGateway(
         subject: subject,
         username: username,
         createdAt: _seed.Customer5.CreatedAt,
         adminRights: 0
      );
      
      // Act
      var result = await _sut.ExecuteAsync(id, CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);
      var CustomerId = result.Value;
      Assert.NotEqual(Guid.Empty, CustomerId);

      var actual = await _repository.FindByIdAsync(CustomerId, CancellationToken.None);
      Assert.NotNull(actual);
      
      Assert.Equal(username, actual.Email);
      Assert.Equal(subject, actual.Subject);
      Assert.Equal(createdAt, actual.CreatedAt);
      
   }
}