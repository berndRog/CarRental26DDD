using CarRentalApi.BuildingBlocks;
namespace CarRentalApi.BuildingBlocks;

/// <summary>
/// Centralized logging extensions for Result and Result&lt;T&gt;.
/// Logs failures consistently at the system boundary.
/// </summary>
public static class ResultLoggingExtensions {

   public static Result LogIfFailure(
      this Result result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      if (result.IsFailure && result.Error is not null) {
         logger.LogWarning(
            "{Context} failed. Code={Code}, Title={Title}, Message={Message}, Args={Args}",
            context,
            result.Error.Code,
            result.Error.Title,
            result.Error.Message,
            args
         );
      }
      return result;
   }

   public static Result<T> LogIfFailure<T>(
      this Result<T> result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      if (result.IsFailure && result.Error is not null) {
         logger.LogWarning(
            "{Context} failed. Code={Code}, Title={Title}, Message={Message}, Args={Args}",
            context,
            result.Error.Code,
            result.Error.Title,
            result.Error.Message,
            args
         );
      }
      return result;
   }
}
