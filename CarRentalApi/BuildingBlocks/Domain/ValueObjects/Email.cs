using CarRentalApi.BuildingBlocks;
namespace CarRentalApi.Modules.Common.Domain.ValueObjects;

public sealed record Email {

   public string Value { get; private set; } = string.Empty;

   // EF Core ctor
   private Email() { }

   // Domain ctor
   private Email(string value) {
      Value = value;
   }

   public static Result<Email> Create(string input) {
      var v = (input ?? string.Empty).Trim().ToLowerInvariant();

      if (v.Length is < 5 or > 320)
         return Result<Email>.Failure(CommonErrors.InvalidEmail);

      var parts = v.Split('@');
      if (parts.Length != 2)
         return Result<Email>.Failure(CommonErrors.InvalidEmail);

      if (string.IsNullOrWhiteSpace(parts[0]) ||
          string.IsNullOrWhiteSpace(parts[1]) ||
          !parts[1].Contains('.'))
         return Result<Email>.Failure(CommonErrors.InvalidEmail);


      return Result<Email>.Success(new Email(v));
   }

   public override string ToString() => Value;
}
