using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;

public sealed record Email {
   public string Value { get; private set; } = string.Empty;

   // EF Core ctor
   private Email() {
   }

   // Domain ctor
   private Email(string value) {
      Value = value;
   }

   public static Result<Email> Create(string input) {
      var v = (input ?? string.Empty).Trim().ToLowerInvariant();

      if (v.Length is < 5 or > 320)
         return Result<Email>.Failure(CommonErrors.InvalidEmail);

      var at = v.LastIndexOf('@');

      // must contain exactly one '@'
      if (at <= 0 || at != v.IndexOf('@') || at >= v.Length - 1)
         return Result<Email>.Failure(CommonErrors.InvalidEmail);

      var localPart = v[..at];
      var domainPart = v[(at + 1)..];

      if (string.IsNullOrWhiteSpace(localPart))
         return Result<Email>.Failure(CommonErrors.InvalidEmail);

      // minimal domain check
      if (!domainPart.Contains('.') ||
          domainPart.StartsWith('.') ||
          domainPart.EndsWith('.'))
         return Result<Email>.Failure(CommonErrors.InvalidEmail);

      return Result<Email>.Success(new Email(v));
   }

   public override string ToString() => Value;
   
}