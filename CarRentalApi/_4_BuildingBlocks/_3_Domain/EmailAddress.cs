using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._4_BuildingBlocks._3_Domain;

public static class EmailAddress {

   public static Result<string> Check(string input) {
      var v = (input ?? string.Empty).Trim().ToLowerInvariant();

      if (v.Length is < 5 or > 320)
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      var at = v.LastIndexOf('@');

      // must contain exactly one '@'
      if (at <= 0 || at != v.IndexOf('@') || at >= v.Length - 1)
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      var localPart = v[..at];
      var domainPart = v[(at + 1)..];

      if (string.IsNullOrWhiteSpace(localPart))
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      // minimal domain check
      if (!domainPart.Contains('.') ||
          domainPart.StartsWith('.') ||
          domainPart.EndsWith('.'))
         return Result<string>.Failure(CommonErrors.InvalidEmail);

      return Result<string>.Success(v);
   }

}