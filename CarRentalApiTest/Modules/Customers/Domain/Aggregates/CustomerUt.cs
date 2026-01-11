using CarRentalApi.BuildingBlocks;
using CarRentalApi.Modules.Common.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
namespace CarRentalApiTest.Modules.Customers.Domain;

/*
public class CustomerUt {

   private readonly TestSeed _seed = new();
   private readonly IClock _clock = new FakeClock();

   [Fact]
   public void Create_WithValidDataAndAddress_ShouldSucceed() {
      // Arrange
      var firstName = _seed.Customer1.FirstName;
      var lastName = _seed.Customer1.LastName;
      var email = _seed.Customer1.Email;
      var createdAt = _seed.Customer1.CreatedAt;
      var address = Address.Create("Teststr. 1", "12345", "TestCity").GetValueOrThrow();
      // Act
      var result = Customer.Create(
         firstName,
         lastName,
         email,
         createdAt,
         Guid.NewGuid().ToString(),
         address
      );
      // Assert
      Assert.True(result.IsSuccess);
      var customer = result.Value;
      Assert.Equal(firstName, customer.FirstName);
      Assert.Equal(lastName, customer.LastName);
      Assert.Equal(email, customer.Email);
      Assert.Equal(createdAt, customer.CreatedAt);
      Assert.Equal(address, customer.Address);
   }

   [Fact]
   public void Create_WithoutAddress_ShouldSucceed() {
      // Arrange
      var firstName = _seed.Customer1.FirstName;
      var lastName = _seed.Customer1.LastName;
      var email = _seed.Customer1.Email;
      var createdAt = _seed.Customer1.CreatedAt;
      // Act
      var result = Customer.Create(
         firstName,
         lastName,
         email,
         createdAt
      );
      // Assert
      Assert.True(result.IsSuccess);
      var customer = result.Value;
      Assert.Equal(firstName, customer.FirstName);
      Assert.Equal(lastName, customer.LastName);
      Assert.Equal(email, customer.Email);
      Assert.Equal(createdAt, customer.CreatedAt);
      Assert.Null(result.Value.Address);
   }

   [Theory]
   [InlineData("", "Mustermann", "e.mustermann@t-line.de")]
   [InlineData("Erika", "", "e.mustermann@t-line.de")]
   [InlineData("Erika", "Mustermann", "")]
   public void Create_WithMissingRequiredData_ShouldFail(
      string firstName,
      string lastName,
      string email
   ) {
      // Act
      var result = Customer.Create(firstName, lastName, email, _clock.UtcNow);

      // Assert
      Assert.True(result.IsFailure);
   }

   [Theory]
   [InlineData("invalid-email")]
   [InlineData("@example.com")]
   [InlineData("user@")]
   [InlineData("user@domain")]
   public void Create_WithInvalidEmailFormat_ShouldFail(string email) {
      // Act
      var result = Customer.Create(
         "Erika",
         "Mustermann",
         email,
         _clock.UtcNow
      );

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(PersonErrors.EmailInvalidFormat, result.Error);
   }

   [Fact]
   public void ChangeEmail_WithValidEmail_ShouldSucceed() {
      // Arrange
      var customer = _seed.Customer1;
      var newEmail = "new.email@example.com";

      // Act
      var result = customer.ChangeEmail(newEmail);

      // Assert
      Assert.True(result.IsSuccess);
      Assert.Equal(newEmail, customer.Email);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("invalid-email")]
   [InlineData("@example.com")]
   public void ChangeEmail_WithInvalidEmail_ShouldFail(string email) {
      // Arrange
      var originalEmail = _seed.Customer1.Email;
      var customer = _seed.Customer1;

      // Act
      var result = customer.ChangeEmail(email);

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(originalEmail, customer.Email); // Unchanged
   }

   [Fact]
   public void Equals_WithSameId_ShouldBeTrue() {
      // Arrange
      var customer1 = _seed.Customer1;
      var customer2 = Customer.Create(
         "Different",
         "Name",
         "different@email.com",
         _clock.UtcNow,
         _seed.Customer1Id
      ).GetValueOrThrow();

      // Act & Assert
      Assert.Equal(customer1, customer2); // Same ID
      Assert.Equal(customer1.GetHashCode(), customer2.GetHashCode());
   }

   [Fact]
   public void Equals_WithDifferentId_ShouldBeFalse() {
      // Arrange
      var customer1 = _seed.Customer1;
      var customer2 = _seed.Customer2;

      // Act & Assert
      Assert.NotEqual(customer1, customer2); // Different ID
   }

   [Fact]
   public void TestSeed_AllCustomers_ShouldBeValid() {
      // Assert
      Assert.Equal("Erika", _seed.Customer1.FirstName);
      Assert.Equal(_seed.Address1, _seed.Customer1.Address);

      Assert.Equal("Max", _seed.Customer2.FirstName);
      Assert.Null(_seed.Customer2.Address);

      Assert.Equal("Arne", _seed.Customer3.FirstName);
      Assert.Equal(_seed.Address2, _seed.Customer3.Address);

      Assert.Equal("Benno", _seed.Customer4.FirstName);
      Assert.Null(_seed.Customer4.Address);

      Assert.Equal("Chrisitine", _seed.Customer5.FirstName);
      Assert.Equal(_seed.Address3, _seed.Customer5.Address);
   }
}
*/