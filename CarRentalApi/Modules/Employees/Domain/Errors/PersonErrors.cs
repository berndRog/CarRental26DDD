using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
namespace CarRentalApi.Modules.Common.Domain.Errors;

/// <summary>
/// Domain-level error definitions for person-related validation rules.
/// </summary>
public static class PersonErrors {

   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadRequest,
         Title: "Invalid Person ReservationId",
         Message: "The Provided ReservationId Is Invalid."
      );
 }

