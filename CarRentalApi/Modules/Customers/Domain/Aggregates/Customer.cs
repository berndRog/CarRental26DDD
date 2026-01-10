using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.Modules.Common.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
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
   
   public DateTimeOffset CreatedAt { get; private set; }
   public DateTimeOffset? BlockedAt { get; private set; }
   public bool IsBlocked => BlockedAt is not null;
   
   // EF Core ctor
   private Customer() { }

   // Domain ctor
   private Customer(
      Guid id,
      string firstName,
      string lastName,
      string email,
      DateTimeOffset createdAt,
      Address? address
   ) : base(id, firstName, lastName, email, address) {
      CreatedAt = createdAt;
   }

   // ---------- Factory (Result-based) ----------
   public static Result<Customer> Create(
      string firstName,
      string lastName,
      string email,
      DateTimeOffset createdAt,
      string? id = null,
      Address? address = null
   ) {
      // Normalize input early
      firstName = firstName?.Trim() ?? string.Empty;
      lastName = lastName?.Trim() ?? string.Empty;
      email = email?.Trim() ?? string.Empty;
      
      var baseValidation = ValidatePersonData(firstName, lastName, email);
      if (baseValidation.IsFailure)
         return Result<Customer>.Failure(baseValidation.Error);
      
      var result = EntityId.Resolve(id, PersonErrors.InvalidId);
      if (result.IsFailure)
         return Result<Customer>.Failure(result.Error);
      var customerId = result.Value;

      var customer = new Customer(
         customerId,
         firstName,
         lastName,
         email,
         createdAt,
         address
      );

      return Result<Customer>.Success(customer);
   }
   
   // ---------- Behavior methods ----------
   public Result ChangeEmail(string email) {
      var validation = ValidatePersonData(FirstName, LastName, email);
      if (validation.IsFailure)
         return validation;

      Email = email.Trim();
      return Result.Success();
   }
   
   public Result Block(
      DateTimeOffset blockedAt
   ) {
      if (IsBlocked)
         return Result.Failure(CustomerErrors.AlreadyBlocked);

      BlockedAt = blockedAt;
      return Result.Success();
   }

}
