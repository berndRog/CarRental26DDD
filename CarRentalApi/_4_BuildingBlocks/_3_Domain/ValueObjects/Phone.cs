using System.Text.RegularExpressions;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;

public sealed record Phone {
   
   public string Number { get; private set; } = string.Empty; // original input
   public string Normalized { get; private set; } = string.Empty;

   // EF Core ctor
   private Phone() { }

   // Domain ctor
   private Phone(string number, string normalized) {
      Number = number;
      Normalized = normalized;
   }
   
   public static Result<Phone?> Create(string? phoneString) {
      
      // null is allowed (optional) no phone provided
      if( phoneString == null)
         return Result<Phone?>.Success(null); // optional
      
      // empty or whitespace is invalid
      if (string.IsNullOrWhiteSpace(phoneString))
         return Result<Phone?>.Failure(CommonErrors.InvalidPhone);

      
      var number = phoneString.Trim();
      
      // allowed characters check
      if (!Allowed.IsMatch(number))
         return Result<Phone?>.Failure(CommonErrors.InvalidPhone);

      // remember if starts with +
      var hasPlus = number.StartsWith("+");
      
      // Remove "(0)" occurrences like "+49 (0)511 ..."
      var cleaned = OptionalTrunkZero.Replace(number, "");
      // Keep digits only
      var digits = Regex.Replace(cleaned, @"\D", ""); 
      
      // sanity: ensure at least 7 digits after normalization
      if (digits.Length < 7)
         return Result<Phone?>.Failure(CommonErrors.InvalidPhone);
      
      // Minimal normalization (canonical form):
      // "+49 (0)511/ 8743 422" -> "+49511812345678"
      var normalized = hasPlus ? "+" + digits : digits;
      
      return Result<Phone?>.Success(new Phone(number, normalized));
   }
   
   // Accept: digits, space, +, (), /, -
   // +49 (0)511 / 1234-5678
   private static readonly Regex Allowed =
      new(@"^(?=.*\d)[0-9 +()/\-]{7,30}$", RegexOptions.Compiled);

   // common international notation artifact: "+49 (0)..."  -> "+49 ..."
   private static readonly Regex OptionalTrunkZero =
      new(@"\(\s*0\s*\)", RegexOptions.Compiled);

   public override string ToString() => Number;
   public string ToNormalized() => Normalized;
}
