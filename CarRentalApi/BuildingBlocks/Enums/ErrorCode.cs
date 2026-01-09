namespace CarRentalApi.BuildingBlocks.Enums;

public enum ErrorCode: Int32 {
   Ok = 200,
   BadReqest = 400,
   Unauthorized = 401,
   Forbidden = 403,
   NotFound = 404,
   Conflict = 409,
   UnsupportedMediaType = 415,
   UnprocessableEntity = 422,
}