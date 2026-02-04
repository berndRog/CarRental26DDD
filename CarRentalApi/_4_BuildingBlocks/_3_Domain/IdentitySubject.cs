using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;

public static class IdentitySubject {
   
   // default system identity, without any IAM
   public static string System() => "system";
   
   public static Result<string> Check(string input) {
      if (string.IsNullOrWhiteSpace(input))
         return Result<string>.Failure(CommonErrors.InvalidIdentitySubject);
      if (input.Length > 200)
         return Result<string>.Failure(CommonErrors.InvalidIdentitySubject);

      /// Identity subject as issued by IAM (opaque, not interpreted).
      return Result<string>.Success(input);
   }
}