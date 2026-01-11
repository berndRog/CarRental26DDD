using System.Text.RegularExpressions;
using CarRentalApi.Modules.Common.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
namespace CarRentalApi.BuildingBlocks.Domain.Entities;

public abstract class Person : Entity<Guid> {
   
   // Guid ReservationId is inherited from Entity<T>
   
   public string FirstName { get; protected set; } = string.Empty;
   public string LastName  { get; protected set; } = string.Empty;
   public string Email     { get; protected set; } = string.Empty;

   // Owned value object (nullable)
   public Address? Address { get; protected set; }

   // EF Core ctor
   protected Person() { }

   // Domain ctor
   protected Person(
      Guid id,
      string firstName,
      string lastName,
      string email,
      Address? address
   ) {
      Id = id;
      FirstName = firstName;
      LastName = lastName;
      Email = email;
      Address = address;
   }

   // derived types (Employee/Admin) can reuse the same validation.
   protected static Result ValidatePersonData(
      string firstName, 
      string lastName, 
      string email
   ) {
      
      if (string.IsNullOrWhiteSpace(firstName))
         return Result.Failure(PersonErrors.FirstNameIsRequired);

      if (string.IsNullOrWhiteSpace(lastName))
         return Result.Failure(PersonErrors.LastNameIsRequired);

      if (string.IsNullOrWhiteSpace(email))
         return Result.Failure(PersonErrors.EmailIsRequired);

      if (!EmailRegex().IsMatch(email.Trim()))
         return Result.Failure(PersonErrors.EmailInvalidFormat);
      
      return Result.Success();
      
   }

   public Result ChangeAddress(Address? address) {
      Address = address;
      return Result.Success();
   }

 
   private static Regex EmailRegex() =>
      new(@"^\S+@\S+\.\S+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
}
