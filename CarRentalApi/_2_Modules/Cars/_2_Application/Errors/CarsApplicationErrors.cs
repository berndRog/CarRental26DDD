using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._2_Modules.Cars._2_Application.Errors;

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
         Message: "The start date must be before the end date."
      );

   public static readonly DomainErrors InvalidExamplesPerCategory =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Examples Per Category",
         Message: "The number of example vehicles per category must be zero or greater."
      );

}
