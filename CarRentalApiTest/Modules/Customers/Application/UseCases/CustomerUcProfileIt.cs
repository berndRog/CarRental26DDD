using CarRentalApi._2_Modules.Customers._1_Ports.Outbound;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;
using CarRentalApi._2_Modules.Customers._2_Application.UseCases;
using CarRentalApi._3_Infrastructure.Persistence.Database;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Modules.Customers.Application.UseCases;

public sealed class CustomerUcProfileIt : TestBase, IAsyncLifetime {
   private SqliteConnection _dbConnection = null!;
   private CarRentalDbContext _dbContext = null!;
   private ICustomerRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private IIdentityGateway _identityGateway = null!;
   private CustomerUcProvision _customerUcProvision = null!;
   private CustomerUcProfile _sut = null!;
   private TestSeed _seed = null!;
   private string _dbPath = null!;
   
   private Guid _customerId;
   private string _id = default!;
   private string _subject = default!;
   private string _username = default!;
   private DateTimeOffset _createdAt;
   private int _adminRights;
   

   public async Task InitializeAsync() {
      _seed = new TestSeed();

      var testDbDir = Path.Combine(AppContext.BaseDirectory, "TestDbs");
      Directory.CreateDirectory(testDbDir);

      _dbPath = Path.Combine(testDbDir, "customers-profile-it.db");

      if (File.Exists(_dbPath))
         File.Delete(_dbPath);

      _dbConnection = new SqliteConnection($"Data Source={_dbPath}");
      await _dbConnection.OpenAsync();

      var options = new DbContextOptionsBuilder<CarRentalDbContext>()
         .UseSqlite(_dbConnection)
         .EnableSensitiveDataLogging()
         .Options;

      _dbContext = new CarRentalDbContext(options);
      await _dbContext.Database.MigrateAsync();

      _repository = new CustomerRepositoryEf(_dbContext);
      _unitOfWork = new UnitOfWork(_dbContext, CreateLogger<UnitOfWork>());

      // seed: provisioned customers
      _repository.Add(_seed.Customer1);
      _repository.Add(_seed.Customer2);
      _repository.Add(_seed.Customer3);
      await _unitOfWork.SaveAllChangesAsync("Seed customers", CancellationToken.None);

      // default gateway for success tests: subject of Customer5, not an employee/admin
      _customerId = _seed.Customer5.Id;
      _id = _customerId.ToString();
      _subject = _seed.Customer5.Subject;
      _username = _seed.Customer5.Email;
      _createdAt = _seed.Customer5.CreatedAt;
      _adminRights = 0;
      _identityGateway = new FakeIdentityGateway(
         subject: _subject,
         username: _username,
         createdAt: _createdAt,
         adminRights: _adminRights
      );

      _customerUcProvision = new CustomerUcProvision(
         _identityGateway,
         _repository,
         _unitOfWork,
         CreateLogger<CustomerUcProvision>()
      );
      
      // system under test
      _sut = new CustomerUcProfile(
         _identityGateway,
         _repository,
         _unitOfWork,
         CreateLogger<CustomerUcProfile>()
      );
   }

   public async Task DisposeAsync() {
      if (_dbContext != null)
         await _dbContext.DisposeAsync();

      if (_dbConnection != null) {
         await _dbConnection.CloseAsync();
         await _dbConnection.DisposeAsync();
      }

      if (!string.IsNullOrWhiteSpace(_dbPath) && File.Exists(_dbPath))
         File.Delete(_dbPath);
   }

   [Fact]
   public async Task ExecuteAsync_WithValidData_ShouldUpdateProfile() {
      // Arrange
      // ensure provisioned
      await _customerUcProvision.ExecuteAsync(_id,CancellationToken.None);

      // new profile data    
      var firstname = _seed.Customer5.Firstname;
      var lastname = _seed.Customer5.Lastname;
      var email = "neue.mail@mail.local";
      var street = _seed.Customer5.Address?.Street;
      var postalCode = _seed.Customer5.Address?.PostalCode;
      var city = _seed.Customer5.Address?.City;
      var country = _seed.Customer5.Address?.Country;
      var dto = new CustomerProfileDto(firstname, lastname, email,
         street, postalCode, city, country);

      // Act
      var resultProfile = await _sut.ExecuteAsync(dto, CancellationToken.None);

      // Assert
      Assert.True(resultProfile.IsSuccess);
      var actual = await _repository.FindByIdAsync(_customerId, CancellationToken.None);
      
      Assert.NotNull(actual);
      Assert.Equal(_customerId, actual.Id);
      Assert.Equal(firstname, actual!.Firstname);
      Assert.Equal(lastname, actual.Lastname);
      Assert.Equal(email, actual.Email);
      Assert.Equal(_subject, actual.Subject);
      Assert.Equal(_createdAt, actual.CreatedAt);
      Assert.Equal(street, actual.Address?.Street);
      Assert.Equal(postalCode, actual.Address?.PostalCode);
      Assert.Equal(city, actual.Address?.City);
      Assert.Equal(country, actual.Address?.Country);
      
   }
/*
   [Fact]
   public async Task ExecuteAsync_WhenNotProvisioned_ShouldFail() {
      _identityGateway = new FakeIdentityGateway(
         subject: Guid.NewGuid().ToString("N"),
         username: "nobody@example.com",
         createdAt: DateTimeOffset.UtcNow,
         adminRights: 0
      );

      _sut = new CustomerUcProfile(
         _repository,
         _identityGateway,
         _unitOfWork,
         CreateLogger<CustomerUcProfile>()
      );

      var dto = new CustomerProfileDto {
         Firstname = "A",
         Lastname = "B",
         EmailString = "a.b@example.com",
         Street = "X",
         PostalCode = "1",
         City = "Y",
         Country = "DE"
      };

      var result = await _sut.ExecuteAsync(dto, CancellationToken.None);

      Assert.True(result.IsFailure);
   }

   [Fact]
   public async Task ExecuteAsync_WhenEmployeeOrAdmin_ShouldFail() {
      _identityGateway = new FakeIdentityGateway(
         subject: _seed.Customer1.Subject.Value,
         username: _seed.Customer1.Email.Value,
         createdAt: _seed.Customer1.CreatedAt,
         adminRights: 1
      );

      _sut = new CustomerUcProfile(
         _repository,
         _identityGateway,
         _unitOfWork,
         CreateLogger<CustomerUcProfile>()
      );

      var dto = new CustomerProfileDto {
         Firstname = "Max",
         Lastname = "Mustermann",
         EmailString = _seed.Customer1.Email.Value,
         Street = "Main Street 1",
         PostalCode = "10115",
         City = "Berlin",
         Country = "DE"
      };

      var result = await _sut.ExecuteAsync(dto, CancellationToken.None);

      Assert.True(result.IsFailure);
   }

   [Fact]
   public async Task ExecuteAsync_WhenEmailAlreadyInUse_ShouldFail() {
      // try to set Customer1 email to Customer2 email
      var dto = new CustomerProfileDto {
         Firstname = _seed.Customer1.Firstname,
         Lastname = _seed.Customer1.Lastname,
         EmailString = _seed.Customer2.Email.Value,
         Street = "Main Street 1",
         PostalCode = "10115",
         City = "Berlin",
         Country = "DE"
      };

      var result = await _sut.ExecuteAsync(dto, CancellationToken.None);

      Assert.True(result.IsFailure);
   }
   */
}