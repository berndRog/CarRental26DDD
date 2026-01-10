using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.BuildingBlocks;

// Nicht-generisches Result für Operationen ohne Rückgabewert
// public readonly struct Result {
//
//    public bool IsFailure { get; }
//    public bool IsSuccess => !IsFailure;
//    public DomainErrors Error { get; }
//
//    private Result(bool isFailure, DomainErrors error) {
//       IsFailure = isFailure;
//       Error = error;
//    }
//
//    public static Result Success() => new(false, DomainErrors.None);
//    public static Result Failure(DomainErrors error) => new(true, error);
//
// }


public sealed class Result {

   public bool IsSuccess { get; }
   public bool IsFailure => !IsSuccess;
   public DomainErrors Error { get; }

   private Result(bool isSuccess, DomainErrors error) {
      IsSuccess = isSuccess;
      Error = error;
   }

   public static Result Success() => new(true, DomainErrors.None);
   public static Result Failure(DomainErrors error) => new(false, error);
}

// Generisches Result<T>:  Echte binäres Zustände
public sealed class Result<T> {
   
   public bool IsFailure { get; }
   public bool IsSuccess => !IsFailure;

   public DomainErrors Error { get; }
   public T Value { get; }

   private Result(bool isFailure, T value, DomainErrors error) {
      IsFailure = isFailure;
      Value = value;
      Error = error;
   }

   public static Result<T> Success(T value) => new(false, value, DomainErrors.None);
   public static Result<T> Failure(DomainErrors error) => new(true, default!, error);
   
   public T GetValueOrDefault(T defaultValue = default!) {
      return IsSuccess && Value is not null ? Value : defaultValue;
   }

   public T GetValueOrThrow() {
      if (!IsSuccess || Value is null)
         throw new InvalidOperationException($"Result failed: {Error}");
      return Value;
   }
   
   public Result<T> OnSuccess(Action<T> action) {
      if (IsSuccess && Value is not null)
         action(Value);
      return this;
   }

   public Result<T> OnFailure(Action<DomainErrors> action) {
      if (!IsSuccess && Error is not null)
         action(Error);
      return this;
   }
   
   public TResult Fold<TResult>(
      Func<T, TResult> onSuccess,
      Func<DomainErrors, TResult> onFailure
   ) {
      return IsSuccess && Value is not null
         ? onSuccess(Value)
         : onFailure(Error!);
   }
   /*
   public Result<TResult> Map<TResult>(Func<T, TResult> mapper) {
      return IsSuccess && Value is not null
         ? Result<TResult>.Success(mapper(Value))
         : Result<TResult>.Failure(Error!);
   }
   
   public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder) {
      return IsSuccess && Value is not null
         ? binder(Value)
         : Result<TResult>.Failure(Error!);
   }
   */
}