using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Cars.Application.ReadModel.Errors;

/// <summary>
/// Domain-level errors used by Cars read queries.
/// </summary>
public static class CarsApplicationErrors {
   
   public static readonly DomainErrors InvalidLimit =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Limit",
         Message: "The Limit Must Be Greater Than Zero."
      );
   public static readonly DomainErrors StartInPast =
      new(
         ErrorCode.BadRequest,
         Title: "Start Date In The Past",
         Message: "The Rental Start Date Must Be In The Future."
      );
   
   public static readonly DomainErrors InvalidPeriod =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Rental Period",
         Message: "The rental start date must be before the end date."
      );

   public static readonly DomainErrors InvalidExamplesPerCategory =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Examples Per Category",
         Message: "The number of example vehicles per category must be zero or greater."
      );

}
