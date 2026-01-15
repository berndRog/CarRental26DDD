using System.Text.RegularExpressions;
namespace CarRentalApi.BuildingBlocks.Domain.ValueObjects;

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

   // Accept: digits, space, +, (), /, -
   // +49 (0)511 / 1234-5678
   private static readonly Regex Allowed =
      new(@"^(?=.*\d)[0-9 +()/\-]{7,30}$", RegexOptions.Compiled);

   public static Result<Phone> Create(string input) {
      if (string.IsNullOrWhiteSpace(input))
         return Result<Phone>.Failure(CommonErrors.InvalidPhone);

      var number = input.Trim();

      if (!Allowed.IsMatch(number))
         return Result<Phone>.Failure(CommonErrors.InvalidPhone);

      var hasPlus = number.StartsWith("+");
      var digits = Regex.Replace(number, @"\D", ""); // keep digits only
      // sanity: ensure at least 7 digits after normalization 
      if (digits.Length < 7)
         return Result<Phone>.Failure(CommonErrors.InvalidPhone);
      
      // Minimal normalization:
      // "+49 (0)511/ 8743 422" -> "+49511812345678"
      var normalized = hasPlus ? "+" + digits : digits;
      
      return Result<Phone>.Success(new Phone(number, normalized));
   }
   
   public override string ToString() => Number;
   public string ToNormalized() => Normalized;
}
