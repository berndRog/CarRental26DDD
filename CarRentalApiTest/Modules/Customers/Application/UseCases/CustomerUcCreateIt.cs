using CarRentalApi._2_Modules.Customers._1_Ports.Outbound;
using CarRentalApi._2_Modules.Customers._2_Application.UseCases;
using CarRentalApi._3_Infrastructure.Persistence.Database;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApiTest.Modules.Customers.Application.UseCases;

public sealed class CustomerCreateIt : TestBase, IAsyncLifetime {
   private SqliteConnection _dbConnection = null!;
   private CarRentalDbContext _dbContext = null!;
   private ICustomerRepository _repository = null!;
   private IUnitOfWork _unitOfWork = null!;
   private CustomerUcCreate _sut = null!;
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

      _repository = new CustomerRepositoryEf(_dbContext);
      _unitOfWork = new UnitOfWork(_dbContext, CreateLogger<UnitOfWork>());

      // Seed cars from TestSeed
      _repository.Add(_seed.Customer1);
      _repository.Add(_seed.Customer2);
      _repository.Add(_seed.Customer3);
      await _unitOfWork.SaveAllChangesAsync("Seed cars", CancellationToken.None);

      _sut = new CustomerUcCreate(
         _repository,
         _unitOfWork,
         CreateLogger<CustomerUcCreate>()
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
   }

   [Fact]
   public async Task ExecuteAsync_WithValidData_ShouldPersistCustomerWithAddress() {
      // Arrange
      var id = _seed.Customer5.Id;
      var firstname = _seed.Customer5.Firstname;
      var lastname = _seed.Customer5.Lastname;
      var email = _seed.Customer5.Email;
      var identitySubject = _seed.Customer5.Subject;
      var createdAt = _seed.Customer5.CreatedAt;
      var address = _seed.Customer5.Address;

      // Act
      var result = await _sut.ExecuteAsync(firstname, lastname, email, identitySubject,
         createdAt, id.ToString(), 
         address?.Street, address?.PostalCode, address?.City, address?.Country,
         CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);
      var customerId = result.Value;
      Assert.NotEqual(Guid.Empty, customerId);

      var actual = await _repository.FindByIdAsync(customerId, CancellationToken.None);
      Assert.NotNull(actual);

      Assert.Equal(customerId, actual.Id);
      Assert.Equal(firstname, actual.Firstname);
      Assert.Equal(lastname, actual.Lastname);
      Assert.Equal(email, actual.Email);
      Assert.Equal(identitySubject, actual.Subject);
      Assert.Equal(createdAt, actual.CreatedAt);
      Assert.Equal(address?.Street, actual.Address?.Street);
      Assert.Equal(address?.PostalCode, actual.Address?.PostalCode);
      Assert.Equal(address?.City, actual.Address?.City);
      Assert.Equal(address?.Country, actual.Address?.Country);
   }
   
   [Fact]
   public async Task ExecuteAsync_WithValidData_ShouldPersistCustomerWithoutAddress() {
      // Arrange
      var id = _seed.Customer4.Id;
      var firstname = _seed.Customer4.Firstname;
      var lastname = _seed.Customer4.Lastname;
      var email = _seed.Customer4.Email;
      var identitySubject = _seed.Customer4.Subject;
      var createdAt = _seed.Customer4.CreatedAt;
      var address = _seed.Customer4.Address;

      // Act
      var result = await _sut.ExecuteAsync(firstname, lastname, email, identitySubject,
         createdAt, id.ToString(), 
         address?.Street, address?.PostalCode, address?.City, address?.Country,
         CancellationToken.None);

      // Assert
      Assert.True(result.IsSuccess);
      var customerId = result.Value;
      Assert.NotEqual(Guid.Empty, customerId);

      var actual = await _repository.FindByIdAsync(customerId, CancellationToken.None);
      Assert.NotNull(actual);

      Assert.Equal(customerId, actual.Id);
      Assert.Equal(firstname, actual.Firstname);
      Assert.Equal(lastname, actual.Lastname);
      Assert.Equal(email, actual.Email);
      Assert.Equal(identitySubject, actual.Subject);
      Assert.Equal(createdAt, actual.CreatedAt);
      Assert.Equal(address?.Street, actual.Address?.Street);
      Assert.Equal(address?.PostalCode, actual.Address?.PostalCode);
      Assert.Equal(address?.City, actual.Address?.City);
      Assert.Equal(address?.Country, actual.Address?.Country);

   }
}