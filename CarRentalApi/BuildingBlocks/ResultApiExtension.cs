using CarRentalApi.BuildingBlocks.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApi.BuildingBlocks;

public static class ResultApiExtensions {

   public static ActionResult ToActionResult<T>(
      this ControllerBase controller,
      Result<T> result,
      ILogger logger,
      string context,
      object? args = null
   ) {
      // Success -> Ok StatusCode 200
      if (result.IsSuccess) return controller.Ok(result.Value);
    
      // Failure -> log and map DomainErrors to HTTP StatusCodes
      result.LogIfFailure<T>(logger, context, args);
      var error = result.Error;
      var problemDetails = new ProblemDetails {
         Title = error.Title,
         Detail = error.Message,
         Status = (int)error.Code
      };
      
      return error.Code switch {
         ErrorCode.BadReqest => controller.BadRequest(problemDetails),                   //400
         ErrorCode.Unauthorized => controller.Unauthorized(problemDetails),               //401
         ErrorCode.Forbidden => new ObjectResult(problemDetails) { StatusCode = 403 },    //403
         ErrorCode.NotFound => controller.NotFound(problemDetails),                       //404
         ErrorCode.Conflict => controller.Conflict(problemDetails),                       //409
         ErrorCode.UnsupportedMediaType => controller.StatusCode(415, problemDetails),    //415
         ErrorCode.UnprocessableEntity => controller.UnprocessableEntity(problemDetails), //422
         _ => controller.BadRequest(problemDetails)                                       //400
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

   /// <summary>
   /// Gibt die HTTP-Statuscodes zurück, die für den gegebenen ErrorCode verwendet werden
   /// </summary>
   public static int ToHttpStatusCode(this ErrorCode errorCode) {
      return errorCode switch {
         ErrorCode.BadReqest => StatusCodes.Status400BadRequest,
         ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
         ErrorCode.Forbidden => StatusCodes.Status403Forbidden,
         ErrorCode.NotFound => StatusCodes.Status404NotFound,
         ErrorCode.Conflict => StatusCodes.Status409Conflict,
         ErrorCode.UnsupportedMediaType => StatusCodes.Status415UnsupportedMediaType,
         ErrorCode.UnprocessableEntity => StatusCodes.Status422UnprocessableEntity,
         _ => StatusCodes.Status400BadRequest
      };
   }
}
