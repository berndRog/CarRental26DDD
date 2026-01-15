using System.Text.RegularExpressions;
using CarRentalApi.Modules.Common.Domain.Errors;
using CarRentalApi.Modules.Common.Domain.ValueObjects;
using CarRentalApi.Modules.Customers.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
namespace CarRentalApi.BuildingBlocks.Domain.Entities;

public abstract class Person : Entity<Guid> {
   
   // Guid ReservationId is inherited from Entity<T>
   
   public string FirstName { get; protected set; } = string.Empty;
   public string LastName  { get; protected set; } = string.Empty;
   public Email Email     { get; protected set; } = default!;

   // Owned value object (nullable)
   public Address? Address { get; protected set; }

   // EF Core ctor
   protected Person() { }

   // Domain ctor
   protected Person(
      Guid id,
      string firstName,
      string lastName,
      Email email,
      Address? address
   ) {
      Id = id;
      FirstName = firstName;
      LastName = lastName;
      Email = email;
      Address = address;
   }

   // derived types (Employee) can reuse the same validation.
   protected static Result ValidatePersonData(
      string firstName, 
      string lastName, 
      string emailString
   ) {
      if (string.IsNullOrWhiteSpace(firstName))
         return Result.Failure(CommonErrors.FirstNameIsRequired);
      if (firstName.Length is < 2 or > 100)
         return Result.Failure(CommonErrors.InvalidFirstName);
      
      if (string.IsNullOrWhiteSpace(lastName))
         return Result.Failure(CommonErrors.LastNameIsRequired);
      if (lastName.Length is < 2 or > 100)
         return Result.Failure(CommonErrors.InvalidFirstName);

      if (string.IsNullOrWhiteSpace(emailString))
         return Result.Failure(CommonErrors.EmailIsRequired);
      var resultEmail = Email.Create(emailString);
      if(!resultEmail.IsFailure) 
         return Result.Failure(CommonErrors.InvalidEmail);
      var email = resultEmail.Value!;
      
      return Result.Success();
   }

   public Result ChangeAddress(Address? address) {
      Address = address;
      return Result.Success();
   }
   
   private static Regex EmailRegex() =>
      new(@"^\S+@\S+\.\S+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
}
