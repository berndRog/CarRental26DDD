using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Cars.Application.ReadModel.Errors;

/// <summary>
/// Domain-level errors used by Cars read queries.
/// </summary>
public static class CarsReadErrors {
   
   public static readonly DomainErrors InvalidLimit =
      new(
         ErrorCode.BadReqest,
         Title: "Invalid Limit",
         Message: "The Limit Must Be Greater Than Zero."
      );
   public static readonly DomainErrors StartInPast =
      new(
         ErrorCode.BadReqest,
         Title: "Start Date In The Past",
         Message: "The Rental Start Date Must Be In The Future."
      );

}
