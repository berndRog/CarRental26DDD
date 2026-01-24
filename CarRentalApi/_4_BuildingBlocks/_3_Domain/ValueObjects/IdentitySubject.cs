using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;

public sealed record IdentitySubject {
   
   public string Value { get; }

   private IdentitySubject(string value) {
      Value = value;
   }

   // default system identity, without any IAM
   public static IdentitySubject System()
      => new IdentitySubject("system");
   
   public static Result<IdentitySubject> Create(string input) {
      if (string.IsNullOrWhiteSpace(input))
         return Result<IdentitySubject>.Failure(CommonErrors.InvalidIdentitySubject);
      if (input.Length > 200)
         return Result<IdentitySubject>.Failure(CommonErrors.InvalidIdentitySubject);

      /// Identity subject as issued by IAM (opaque, not interpreted).
      return Result<IdentitySubject>.Success(new IdentitySubject(input));
   }

   public override string ToString() => Value;
}