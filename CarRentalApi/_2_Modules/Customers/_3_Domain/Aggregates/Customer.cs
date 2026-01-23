using CarRentalApi._2_Modules.Customers._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks._3_Domain.Entities;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Domain.ValueObjects;
using CarRentalApi.BuildingBlocks;
namespace CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;

public sealed class Customer : Entity<Guid> {
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

   public string FirstName { get; private set; } = string.Empty;
   public string LastName { get; private set; } = string.Empty;
   public Email Email { get; private set; } = default!;
   public Address? Address { get; private set; }

   public IdentitySubject IdentitySubject { get; private set; } = default!; // OidvOAuthServer

   public DateTimeOffset CreatedAt { get; private set; }
   public DateTimeOffset? BlockedAt { get; private set; }
   public bool IsBlocked => BlockedAt is not null;

   // EF Core ctor
   private Customer() {
   }

   // Domain ctor
   private Customer(
      Guid id,
      string firstName,
      string lastName,
      Email email,
      Address? address,
      IdentitySubject identitySubject,
      DateTimeOffset createdAt
   ) {
      Id = id;
      FirstName = firstName;
      LastName = lastName;
      Email = email;
      Address = address;
      IdentitySubject = identitySubject;
      CreatedAt = createdAt;
   }

   // ---------- Factory (Result-based) ----------
   public static Result<Customer> Create(
      string firstName,
      string lastName,
      string emailString,
      string? phoneString = null,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      DateTimeOffset createdAt = default,
      string? id = null
   ) {
      // Normalize input early
      firstName = firstName.Trim();
      lastName = lastName.Trim();
      emailString = emailString.Trim();

      if (string.IsNullOrWhiteSpace(firstName))
         return Result<Customer>.Failure(CustomerErrors.FirstNameIsRequired);
      if (firstName.Length is < 2 or > 100)
         return Result<Customer>.Failure(CustomerErrors.InvalidFirstName);

      if (string.IsNullOrWhiteSpace(lastName))
         return Result<Customer>.Failure(CustomerErrors.LastNameIsRequired);
      if (lastName.Length is < 2 or > 100)
         return Result<Customer>.Failure(CustomerErrors.InvalidLastName);

      if (string.IsNullOrWhiteSpace(emailString))
         return Result<Customer>.Failure(CustomerErrors.EmailIsRequired);
      var resultEmail = Email.Create(emailString);

      if (!resultEmail.IsFailure)
         return Result<Customer>.Failure(resultEmail.Error);
      var email = resultEmail.Value!;

      Address? address = null;
      if (!string.IsNullOrWhiteSpace(street) &&
          !string.IsNullOrWhiteSpace(postalCode) &&
          !string.IsNullOrWhiteSpace(city)
         ) {
         var addressResult = Address.Create(street, postalCode, city);
         if (addressResult.IsFailure)
            return Result<Customer>.Failure(addressResult.Error);
         address = addressResult.Value;
      }

      var result = EntityId.Resolve(id, CustomerErrors.InvalidId);
      if (result.IsFailure)
         return Result<Customer>.Failure(result.Error);
      var customerId = result.Value;

      var identitySubject = IdentitySubject.System();

      var customer = new Customer(
         id: customerId,
         firstName: firstName,
         lastName: lastName,
         email: email,
         identitySubject: identitySubject,
         address: address,
         createdAt: createdAt
      );

      return Result<Customer>.Success(customer);
   }

   public static Result<Customer> CreateProvisioned(
      IdentitySubject identitySubject,
      Email email,
      DateTimeOffset createdAt,
      Guid? id = null
   ) {
      if (createdAt == default)
         return Result<Customer>.Failure(CustomerErrors.CreatedAtIsRequired);

      // identitySubject/email sind bereits VOs => valid
      var customer = new Customer(
         id ?? Guid.NewGuid(),
         firstName: string.Empty,
         lastName: string.Empty,
         email: email,
         address: null,
         createdAt: createdAt,
         identitySubject: identitySubject
      );

      return Result<Customer>.Success(customer);
   }

   //--- Domain methods ---
   public Result Block(
      DateTimeOffset blockedAt
   ) {
      if (IsBlocked)
         return Result.Failure(CustomerErrors.AlreadyBlocked);

      BlockedAt = blockedAt;
      return Result.Success();
   }
}