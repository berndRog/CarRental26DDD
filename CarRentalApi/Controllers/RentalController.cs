// CarRentalApi/Controllers/RentalsController.cs
using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;
using CarRentalApi.Modules.Bookings.Application.UseCases.Dto;
using CarRentalApi.Modules.Bookings.Domain.Errors;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using CarRentalApi.Modules.Rentals;
using CarRentalApi.Modules.Rentals.Application.ReadModel;
using CarRentalApi.Modules.Rentals.Application.ReadModel.Dto;
using CarRentalApi.Modules.Rentals.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApi.Controllers;

[ApiController]
[Route("carrentalapi/v1")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class RentalsController(
   IRentalReadModel _readModel,
   IRentalUseCases _useCases,
   IClock _clock,
   ILogger<RentalsController> _logger
) : ControllerBase {
   
   // ------------------------------------------------------------
   // GET /api/rentals/{rentalId}
   // ------------------------------------------------------------
   [HttpGet("rentals/{rentalId:guid}", Name = nameof(GetByIdAsync))]
   [ProducesResponseType(typeof(RentalDetailsDto), StatusCodes.Status200OK)]
   public async Task<IActionResult> GetByIdAsync(Guid rentalId, CancellationToken ct) {

      var result = await _readModel.FindByIdAsync(rentalId, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(RentalsController)}.{nameof(GetByIdAsync)}",
         args: new { RentalId = rentalId }
      );
   }

   // ------------------------------------------------------------
   // POST /api/reservations/{reservationId}/rentals/pickup
   // ------------------------------------------------------------
   [HttpPost("reservations/{reservationId:guid}/rentals/pickup", Name = nameof(PickupAsync))]
   [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
   public async Task<IActionResult> PickupAsync(
      Guid reservationId,
      [FromBody] RentalPickupDto pickupDto,
      CancellationToken ct
   ) {
      
      // Guard 1: Route-ID should not be empty
      if (reservationId == Guid.Empty) {
         return this.ToActionResult(
            Result<Guid>.Failure(ReservationApplicationErrors.InvalidId),
            _logger,
            context: $"{nameof(RentalsController)}.{nameof(PickupAsync)}",
            args: new { ReservationId = reservationId }
         );
      }

      // Guard 2: Route-ID must equal to Body-ID
      if (reservationId != pickupDto.ReservationId) {
         return this.ToActionResult(
            Result<Guid>.Failure(ReservationApplicationErrors.ReservationIdMismatch),
            _logger,
            context: $"{nameof(RentalsController)}.{nameof(PickupAsync)}",
            args: new { ReservationId = reservationId, dto = pickupDto }
         );
      }
      
      // If PickedUpAt is not provided, set it to current UTC time
      if (pickupDto.PickedupAt == default) 
         pickupDto = pickupDto with { PickedupAt = _clock.UtcNow };
      
      // Execute use case
      var result = await _useCases.PickupAsync(pickupDto, ct);

      // Return result
      return this.CreatedAt(
         routeName: nameof(GetByIdAsync),
         routeValues: new { rentalId = result.IsSuccess ? result.Value : Guid.Empty },
         result: result,
         logger: _logger,
         context: $"{nameof(RentalsController)}.{nameof(PickupAsync)}",
         args: new { ReservationId = reservationId, dto = pickupDto }
      );
   }

   // ------------------------------------------------------------
   // POST /api/rentals/{rentalId}/return
   // ------------------------------------------------------------
   [HttpPost("rentals/{rentalId:guid}/return", Name = nameof(ReturnAsync))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public async Task<IActionResult> ReturnAsync(
      Guid rentalId,
      [FromBody] RentalReturnDto returnDto,
      CancellationToken ct
   ) {
      // Guard 1: Route-ID should not be empty
      if (rentalId == Guid.Empty) {
         return this.ToActionResult(
            Result.Failure(RentalApplicationErrors.InvalidId),
            _logger,
            context: $"{nameof(RentalsController)}.{nameof(ReturnAsync)}",
            args: new { RentalId = rentalId }
         );
      }

      // Guard 2: Route-ID must equal to Body-ID
      if(rentalId != returnDto.RentalId) {
         return this.ToActionResult(
            Result.Failure(RentalApplicationErrors.RentalIdMismatch),
            _logger,
            context: $"{nameof(RentalsController)}.{nameof(ReturnAsync)}",
            args: new { RentalId = rentalId, dto = returnDto }
         );
      }
      
      // If ReturnAt is not provided, set it to current UTC time
      if (returnDto.ReturnAt == default) 
         returnDto = returnDto with { ReturnAt = _clock.UtcNow };
      
      // Execute use case
      var result = await _useCases.ReturnAsync(returnDto, ct);

      // Return result
      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(RentalsController)}.{nameof(ReturnAsync)}",
         args: new { RentalId = rentalId, dto = returnDto }
      );
   }
}
