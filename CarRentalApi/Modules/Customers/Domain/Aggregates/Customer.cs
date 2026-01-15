using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.Modules.Common.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.Errors;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
namespace CarRentalApi.Modules.Customers.Domain.Aggregates;

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

   public Contact Contact { get; private set; } = default!;
   public Credentials Credentials { get; private set; } = default!; // Phase 1: required
   public string? Identity { get; private set; } // Phase 2 (IAM)
   public Address? Address { get; private set; }
   public DateTimeOffset CreatedAt { get; private set; }
   public DateTimeOffset? BlockedAt { get; private set; }
   public bool IsBlocked => BlockedAt is not null;

   // EF Core ctor
   private Customer() {
   }

   // Domain ctor
   private Customer(
      Guid id,
      Contact contact,
      Address? address,
      DateTimeOffset createdAt
   ) {
      Id = id;
      Contact = contact;
      Address = address;
      CreatedAt = createdAt;
   }

   // ---------- Factory (Result-based) ----------
   public static Result<Customer> Create(
      string firstName,
      string lastName,
      string email,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      DateTimeOffset createdAt = default,
      string? id = null
   ) {
      var resultContact = Contact.Create(firstName, lastName, email);
      if (resultContact.IsFailure)
         return Result<Customer>.Failure(resultContact.Error);
      var contact = resultContact.Value!;

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

      if (createdAt == default)
         return Result<Customer>.Failure(CommonErrors.CreatedAtIsRequired);

      var result = EntityId.Resolve(id, PersonErrors.InvalidId);
      if (result.IsFailure)
         return Result<Customer>.Failure(result.Error);
      var customerId = result.Value;

      var customer = new Customer(
         customerId,
         contact,
         address,
         createdAt
      );

      return Result<Customer>.Success(customer);
   }

   //--- Domain methods ---
   // Registration: Set initial password (registration)
   public Result SetPassword(string plainPassword) {
      var credentialsResult = Credentials.Create(plainPassword);
      if (credentialsResult.IsFailure)
         return Result.Failure(credentialsResult.Error);
      // Set credentials
      Credentials = credentialsResult.Value;
      return Result.Success();
   }

   // Login: Verify password
   public Result VerifyPassword(string plainPassword) {
      if (Credentials is null)
         return Result.Failure(CredentialsErrors.CredentialsNotSet);
      return Credentials.VerifyPassword(plainPassword);
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