using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._4_BuildingBlocks._1_Ports.Inbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApiTest.Modules.Customers.Domain;

public class CustomerUt {

   private readonly TestSeed _seed = new();
   private readonly IClock _clock = new FakeClock();

   [Fact]
   public void Create_WithValidDataAndAddress_ShouldSucceed() {
      // Arrange
      var id = _seed.Customer1.Id;
      var firstname = _seed.Customer1.Firstname;
      var lastname = _seed.Customer1.Lastname;
      var email = _seed.Customer1.Email;
      var subject = _seed.Customer1.Subject;
      var createdAt = _seed.Customer1.CreatedAt;
      var address = _seed.Address1;
      
      // Act
      var result =
         Customer.Create(firstname, lastname, email.Value, subject.Value, createdAt,
            id.ToString(), address.Street, address.PostalCode, address.City, address.Country);
      
      // Assert
      Assert.True(result.IsSuccess);
      var customer = result.Value;
      Assert.Equal(id, customer.Id);
      Assert.Equal(firstname, customer.Firstname);
      Assert.Equal(lastname, customer.Lastname);
      Assert.Equal(email, customer.Email);
      Assert.Equal(subject, customer.Subject);
      Assert.Equal(createdAt, customer.CreatedAt);
      Assert.Equal(createdAt, customer.CreatedAt);
      Assert.Equal(address, customer.Address);
   }

   [Fact]
   public void Create_WithoutAddress_ShouldSucceed() {
      // Arrange
      var id = _seed.Customer1.Id;
      var firstname = _seed.Customer1.Firstname;
      var lastname = _seed.Customer1.Lastname;
      var email = _seed.Customer1.Email;
      var subject = _seed.Customer1.Subject;
      var createdAt = _seed.Customer1.CreatedAt;
      
      // Act
      var result = Customer.Create(firstname, lastname, email.Value, 
         subject.Value, createdAt, id.ToString());
      
      // Assert
      Assert.True(result.IsSuccess);
      var customer = result.Value;
      Assert.Equal(firstname, customer.Firstname);
      Assert.Equal(lastname, customer.Lastname);
      Assert.Equal(email, customer.Email);
      Assert.Equal(subject, customer.Subject);
      Assert.Equal(createdAt, customer.CreatedAt);
      Assert.Null(result.Value.Address);
   }

   [Theory]
   [InlineData("", "Mustermann", "e.mustermann@t-line.de")]
   [InlineData("Erika", "", "e.mustermann@t-line.de")]
   [InlineData("Erika", "Mustermann", "")]
   public void Create_WithMissingRequiredData_ShouldFail(
      string firstname,
      string lastname,
      string emailString
   ) {
      var id = _seed.Customer1.Id;
      var subject = _seed.Customer1.Subject;
      var createdAt = _seed.Customer1.CreatedAt;
      
      // Act
      var result = Customer.Create(firstname, lastname, emailString, 
         subject.Value, createdAt, id.ToString());

      // Assert
      Assert.True(result.IsFailure);
   }

   [Theory]
   [InlineData("invalid-email")]
   [InlineData("@example.com")]
   [InlineData("user@")]
   [InlineData("user@domain")]
   public void Create_WithInvalidEmailFormat_ShouldFail(string emailString) {
      var id = _seed.Customer1.Id;
      var firstname = _seed.Customer1.Firstname;
      var lastname = _seed.Customer1.Lastname;
      var subject = _seed.Customer1.Subject;
      var createdAt = _seed.Customer1.CreatedAt;
     
      // Act
      var result = Customer.Create(firstname,lastname, emailString,
         subject.Value, createdAt, id.ToString());

      // Assert
      Assert.True(result.IsFailure);
      Assert.Equal(CommonErrors.InvalidEmail, result.Error);
   }
   
   
   [Fact]
   public void Equals_WithSameId_ShouldBeTrue() {
      // Arrange
      var customer1 = _seed.Customer1;
      var customer2 = Customer.Create(
         _seed.Customer2.Firstname,
         _seed.Customer2.Lastname,
         _seed.Customer2.Email.Value,
         _seed.Customer2.Subject.Value,
         _seed.Customer2.CreatedAt,
         _seed.Customer1Id.ToString()
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
   
}
