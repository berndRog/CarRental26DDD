namespace CarRentalApi.BuildingBlocks.Domain.ValueObjects;

public sealed record IdentitySubject {
   
   public string Value { get; }

   private IdentitySubject(string value) {
      Value = value;
   }

   public static Result<IdentitySubject> Create(string input) {
      if (string.IsNullOrWhiteSpace(input))
         return Result<IdentitySubject>.Failure(CommonErrors.InvalidIdentitySubject);
      return Result<IdentitySubject>.Success(new IdentitySubject(input));
   }

   public override string ToString() => Value;
}