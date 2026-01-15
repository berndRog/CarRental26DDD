using System.Text.RegularExpressions;
using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Domain.ValueObjects;
using CarRentalApi.Modules.Common.Domain.ValueObjects;
namespace CarRentalApi.Modules.Customers.Domain.ValueObjects;

public sealed record Contact {

   public string FirstName { get; private set; } = string.Empty;
   public string LastName  { get; private set; } = string.Empty;
   public Email Email { get; private set; } = default!;
   public Phone? Phone     { get; private set; }
   
   // EF Core
   private Contact() { }
   
   // Domain ctor
   private Contact(
      string firstName,
      string lastName,
      Email  email,
      Phone? phone
   ) {
      FirstName = firstName;
      LastName  = lastName;
      Email     = email;
      Phone     = phone;
   }

   public static Result<Contact> Create(
      string firstName,
      string lastName,
      string emailString,
      string? phoneString = null
   ) {
      // Normalize input early
      firstName = firstName.Trim();
      lastName = lastName.Trim();
      emailString = emailString.Trim();
      phoneString = phoneString?.Trim();
      
      if (string.IsNullOrWhiteSpace(firstName))
         return Result<Contact>.Failure(CommonErrors.FirstNameIsRequired);
      if (firstName.Length is < 2 or > 100)
         return Result<Contact>.Failure(CommonErrors.InvalidFirstName);
      
      if (string.IsNullOrWhiteSpace(lastName))
         return Result<Contact>.Failure(CommonErrors.LastNameIsRequired);
      if (lastName.Length is < 2 or > 100)
         return Result<Contact>.Failure(CommonErrors.InvalidFirstName);

      if (string.IsNullOrWhiteSpace(emailString))
         return Result<Contact>.Failure(CommonErrors.EmailIsRequired);
      var resultEmail = Email.Create(emailString);
      if(!resultEmail.IsFailure) 
         return Result<Contact>.Failure(CommonErrors.InvalidEmail);
      var email = resultEmail.Value!;
      
      Phone? phone = null;
      if (!string.IsNullOrWhiteSpace(phoneString)) {
         var resultPhone = Phone.Create(phoneString);
         if (!resultPhone.IsFailure)
            return Result<Contact>.Failure(resultPhone.Error);
         phone = resultPhone.Value!;
      }

      return Result<Contact>.Success(
         new Contact(firstName, lastName, email, phone)
      );
   }

   public string FullName => $"{FirstName} {LastName}";
   
   private static Regex EmailRegex() =>
      new(@"^\S+@\S+\.\S+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
   
   private static readonly Regex PhoneRegex =
      new(@"^(?=.*\d)[0-9 +()/\-]{7,30}$", RegexOptions.Compiled);
   
   public static string Normalize(string phone) {
      // keep digits and leading +
      phone = phone.Trim();

      var hasPlus = phone.StartsWith("+");
      var digits = Regex.Replace(phone, @"\D", "");

      return hasPlus ? "+" + digits : digits;
   }

}
