using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using Microsoft.AspNetCore.Mvc;
namespace CarRentalApi._4_BuildingBlocks;

public static class ResultApiExtensions {

   public static ActionResult ToActionResult(
      this ControllerBase controller,
      Result result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      // Success -> NoContent 204 (typisch für Commands ohne Body)
      if (result.IsSuccess)
         return controller.NoContent();

      // Failure -> log and map DomainErrors to HTTP StatusCodes
      result.LogIfFailure(logger, context, args);

      var error = result.Error;
      var problemDetails = new ProblemDetails {
         Title = error.Title,
         Detail = error.Message,
         Status = error.Code.ToHttpStatusCode()
      };

      return error.Code switch {
         ErrorCode.BadRequest =>
            controller.BadRequest(problemDetails),

         ErrorCode.Unauthorized =>
            controller.Unauthorized(problemDetails),

         ErrorCode.Forbidden =>
            controller.StatusCode(StatusCodes.Status403Forbidden, problemDetails),

         ErrorCode.NotFound =>
            controller.NotFound(problemDetails),

         ErrorCode.Conflict =>
            controller.Conflict(problemDetails),

         ErrorCode.UnsupportedMediaType =>
            controller.StatusCode(StatusCodes.Status415UnsupportedMediaType, problemDetails),

         ErrorCode.UnprocessableEntity =>
            controller.UnprocessableEntity(problemDetails),

         _ =>
            controller.BadRequest(problemDetails)
      };
   }

   public static ActionResult ToActionResult<T>(
      this ControllerBase controller,
      Result<T> result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      if (result.IsSuccess)
         return controller.Ok(result.Value);

      result.LogIfFailure(logger, context, args);

      var error = result.Error;
      var problemDetails = new ProblemDetails {
         Title = error.Title,
         Detail = error.Message,
         Status = error.Code.ToHttpStatusCode()
      };

      return error.Code switch {
         ErrorCode.BadRequest =>
            controller.BadRequest(problemDetails),

         ErrorCode.Unauthorized =>
            controller.Unauthorized(problemDetails),

         ErrorCode.Forbidden =>
            controller.StatusCode(StatusCodes.Status403Forbidden, problemDetails),

         ErrorCode.NotFound =>
            controller.NotFound(problemDetails),

         ErrorCode.Conflict =>
            controller.Conflict(problemDetails),

         ErrorCode.UnsupportedMediaType =>
            controller.StatusCode(StatusCodes.Status415UnsupportedMediaType, problemDetails),

         ErrorCode.UnprocessableEntity =>
            controller.UnprocessableEntity(problemDetails),

         _ =>
            controller.BadRequest(problemDetails)
      };
   }

   public static ActionResult CreatedAt<T>(
      this ControllerBase controller,
      string routeName,
      object? routeValues,
      Result<T> result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      if (result.IsFailure)
         return controller.ToActionResult(result, logger, context, args);

      return controller.CreatedAtRoute(routeName, routeValues, result.Value);
   }

   public static int ToHttpStatusCode(this ErrorCode errorCode) =>
      errorCode switch {
         ErrorCode.BadRequest => StatusCodes.Status400BadRequest,
         ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
         ErrorCode.Forbidden => StatusCodes.Status403Forbidden,
         ErrorCode.NotFound => StatusCodes.Status404NotFound,
         ErrorCode.Conflict => StatusCodes.Status409Conflict,
         ErrorCode.UnsupportedMediaType => StatusCodes.Status415UnsupportedMediaType,
         ErrorCode.UnprocessableEntity => StatusCodes.Status422UnprocessableEntity,
         _ => StatusCodes.Status400BadRequest
      };
}
