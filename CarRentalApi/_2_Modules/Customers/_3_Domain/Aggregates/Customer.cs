using CarRentalApi._2_Modules.Customers._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi._4_BuildingBlocks._3_Domain.Entities;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks._3_Domain;
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

   public string Firstname { get; private set; } = string.Empty;
   public string Lastname { get; private set; } = string.Empty;
   public string Email { get; private set; } = default!;
   
   public string Subject { get; private set; } = default!; // OidvOAuthServer
   
   public DateTimeOffset CreatedAt { get; private set; }
   public DateTimeOffset? BlockedAt { get; private set; }
   public bool IsBlocked => BlockedAt is not null;

   public Address? Address { get; private set; }

   // EF Core ctor
   private Customer() {
   }

   // Domain ctor
   private Customer(
      Guid id,
      string firstname,
      string lastname,
      string email,
      string subject,
      DateTimeOffset createdAt,
      Address? address
   ) {
      Id = id;
      Firstname = firstname;
      Lastname = lastname;
      Email = email;
      Subject = subject;
      CreatedAt = createdAt;
      Address = address;
   }

   // ---------- Factory (Result-based) ----------
   public static Result<Customer> Create(
      string firstname,
      string lastname,
      string email,
      string subject = "system",
      DateTimeOffset createdAt = default,
      string? id = null,
      string? street = null,
      string? postalCode = null,
      string? city = null,
      string? country = null
   ) {
      // Normalize input early
      firstname = firstname.Trim();
      lastname = lastname.Trim();
      email = email.Trim();
      subject = subject.Trim();

      if (string.IsNullOrWhiteSpace(firstname))
         return Result<Customer>.Failure(CustomerErrors.FirstnameIsRequired);
      if (firstname.Length is < 2 or > 100)
         return Result<Customer>.Failure(CustomerErrors.InvalidFirstname);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result<Customer>.Failure(CustomerErrors.LastnameIsRequired);
      if (lastname.Length is < 2 or > 100)
         return Result<Customer>.Failure(CustomerErrors.InvalidLastname);

      // create Email value object
      if (string.IsNullOrWhiteSpace(email))
         return Result<Customer>.Failure(CustomerErrors.EmailIsRequired);
      var resultEmail = EmailAddress.Check(email);
      if (resultEmail.IsFailure)
         return Result<Customer>.Failure(resultEmail.Error);
      
      // create IdentitySubject value object
      if (!string.IsNullOrWhiteSpace(subject)) {
         var resultSubject = IdentitySubject.Check(subject);
         if (resultSubject.IsFailure)
            return Result<Customer>.Failure(resultSubject.Error);
      } 

      // create Id:Guid required
      var result = EntityId.Resolve(id, CustomerErrors.InvalidId);
      if (result.IsFailure)
         return Result<Customer>.Failure(result.Error);
      var customerId = result.Value;
      
      // create Address value object (optional)
      Address? address = null;
      if (!string.IsNullOrWhiteSpace(street) &&
          !string.IsNullOrWhiteSpace(postalCode) &&
          !string.IsNullOrWhiteSpace(city)
         ) {
         var addressResult = Address.Create(street, postalCode, city, country);
         if (addressResult.IsFailure)
            return Result<Customer>.Failure(addressResult.Error);
         address = addressResult.Value;
      }
      
      // create new Customer aggregate
      var customer = new Customer(
         id: customerId,
         firstname: firstname,
         lastname: lastname,
         email: email,
         subject: subject,
         createdAt: createdAt,
         address: address
      );

      return Result<Customer>.Success(customer);
   }
   
   public static Result<Customer> CreateProvisioned(
      string identitySubject,
      string email,
      DateTimeOffset createdAt,
      string? id = null
   ) {
      if (createdAt == default)
         return Result<Customer>.Failure(CustomerErrors.CreatedAtIsRequired);

      // create Id:Guid required
      var result = EntityId.Resolve(id, CustomerErrors.InvalidId);
      if (result.IsFailure)
         return Result<Customer>.Failure(result.Error);
      var customerId = result.Value;
      
      // identitySubject/email sind bereits VOs => valid
      var customer = new Customer(
         customerId,
         firstname: string.Empty,
         lastname: string.Empty,
         email: email,
         address: null,
         createdAt: createdAt,
         subject: identitySubject
      );

      return Result<Customer>.Success(customer);
   }

   //--- Domain methods ---
   public Result UpdateProfile(
      string firstname,
      string lastname,
      string email,
      string? street,
      string? postalCode,
      string? city,
      string? country
   ) {
      Firstname = firstname.Trim();
      Lastname  = lastname.Trim();
      
      // Basic required fields
      if (string.IsNullOrWhiteSpace(firstname))
         return Result.Failure(CustomerErrors.FirstnameIsRequired);

      if (string.IsNullOrWhiteSpace(lastname))
         return Result.Failure(CustomerErrors.LastnameIsRequired);
      
      // Email (already validated as value object by caller)
      Email = email;
      
      // Address: either fully set or null (no half-addresses)
      var anyAddress =
         !string.IsNullOrWhiteSpace(street) ||
         !string.IsNullOrWhiteSpace(postalCode) ||
         !string.IsNullOrWhiteSpace(city) ||
         !string.IsNullOrWhiteSpace(country);

      if (!anyAddress) {
         Address = null;
         return Result.Success();
      }

      // require all address fields if one is present
      if (string.IsNullOrWhiteSpace(street))
         return Result.Failure(CommonErrors.StreetIsRequired);
      if (string.IsNullOrWhiteSpace(postalCode))
         return Result.Failure(CommonErrors.PostalCodeIsRequired);
      if (string.IsNullOrWhiteSpace(city))
         return Result.Failure(CommonErrors.CityIsRequired);

      var addressResult = Address.Create(
         street.Trim(),
         postalCode.Trim(),
         city.Trim(),
         country?.Trim()
      );
      if (addressResult.IsFailure)
         return Result.Failure(addressResult.Error);

      Address = addressResult.Value;
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