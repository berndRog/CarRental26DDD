using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.Modules.Common.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Modules.Reservations.Domain.Aggregates;
namespace CarRentalApi.Modules.Customers.Domain.Aggregates;

public sealed class Customer : Person {

#if OOP_MODE
   // With Navigation properties (object graph)
   // Customer <-> Reservation 1 : 0..n
   private readonly List<Reservation> _reservations = new();
   public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();
   // Customer <-> Rental 1 : 0..n
   private readonly List<Rental> _rentals = new();
   public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

#elif DDD_MODE   
   // Without Navigation properties 
   // Use repositories to fetch related Reservation or Rental

#else
   #error "Define either OOP_MODE or DDD_MODE in .csproj"
#endif
   
   // EF Core ctor
   private Customer() { }

   // Domain ctor
   private Customer(
      Guid id,
      string firstName,
      string lastName,
      string email,
      Address? address
   ) : base(id, firstName, lastName, email, address) { }

   // ---------- Factory (Result-based) ----------
   public static Result<Customer> Create(
      string firstName,
      string lastName,
      string email,
      string? id = null,
      Address? address = null
   ) {
      // Normalize input early
      firstName = firstName?.Trim() ?? string.Empty;
      lastName = lastName?.Trim() ?? string.Empty;
      email = email?.Trim() ?? string.Empty;
      
      var baseValidation = ValidatePersonData(firstName, lastName, email);
      if (baseValidation.IsFailure)
         return Result<Customer>.Failure(baseValidation.Error!);
      
      var result = EntityId.Resolve(id, PersonErrors.InvalidId);
      if (result.IsFailure)
         return Result<Customer>.Failure(result.Error!);
      var customerId = result.Value;

      var customer = new Customer(
         customerId,
         firstName,
         lastName,
         email,
         address
      );

      return Result<Customer>.Success(customer);
   }

   // public Result ChangeName(
   //    string firstName,
   //    string lastName
   // ) {
   //    // Email bleibt unverändert -> eigene minimale Validierung
   //    if (string.IsNullOrWhiteSpace(firstName))
   //       return Result.Failure(PersonErrors.FirstNameIsRequired);
   //
   //    if (string.IsNullOrWhiteSpace(lastName))
   //       return Result.Failure(PersonErrors.LastNameIsRequired);
   //
   //    FirstName = firstName.Trim();
   //    LastName  = lastName.Trim();
   //    return Result.Success();
   // }

   public Result ChangeEmail(string email) {
      var validation = ValidatePersonData(FirstName, LastName, email);
      if (validation.IsFailure)
         return validation;

      Email = email.Trim();
      return Result.Success();
   }
}
