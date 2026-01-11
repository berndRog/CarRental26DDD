using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Bookings;
using CarRentalApi.Modules.Bookings.Api.Dto;
using CarRentalApi.Modules.Bookings.Application.Contracts.Dto;
using CarRentalApi.Modules.Bookings.Application.ReadModel;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Dto;
using CarRentalApi.Modules.Bookings.Application.UseCases;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApi.Controllers;

[ApiController]

// 🔹 Enable later if API versioning is introduced
// [ApiVersion("1.0")]

// 🔹 Replace route with:
// [Route("carrentalapi/v{version:apiVersion}")]
[Route("carrentalapi/v1")]

[Consumes("application/json")]
[Produces("application/json")]
public sealed class ReservationsController(
   IReservationReadModel _readModel,
   IReservationUseCases _useCases,
   ILogger<ReservationsController> _logger
) : ControllerBase {
   
   /// <summary>
   /// Returns reservation details by id.
   /// </summary>
   [HttpGet("booking/reservations/{id:guid}", Name = nameof(GetById))]
   [ProducesResponseType(typeof(ReservationDetailsDto), StatusCodes.Status200OK)]
   public async Task<IActionResult> GetById(Guid id, CancellationToken ct) {

      var result = await _readModel.FindByIdAsync(id, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(ReservationsController)}.{nameof(GetById)}",
         args: new {
            ReservationId = id
            // Version = HttpContext.GetRequestedApiVersion()?.ToString()
         }
      );
   }

   /// <summary>
   /// Searches reservations using filter, paging and sorting.
   /// </summary>
   [HttpGet("booking/reservations", Name = nameof(Search))]
   [ProducesResponseType(typeof(PagedResult<ReservationListItemDto>), StatusCodes.Status200OK)]
   public async Task<IActionResult> Search(
      [FromQuery] ReservationSearchFilter filter,
      [FromQuery] PageRequest page,
      [FromQuery] SortRequest sort,
      CancellationToken ct
   ) {
      var result = await _readModel.SearchAsync(filter, page, sort, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(ReservationsController)}.{nameof(Search)}",
         args: new {
            filter,
            page,
            sort
            // Version = HttpContext.GetRequestedApiVersion()?.ToString()
         }
      );
   }

   /// <summary>
   /// Creates a new reservation (draft).
   /// </summary>
   [HttpPost("booking/reservations", Name = nameof(Create))]
   [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
   public async Task<IActionResult> Create(
      [FromBody] ReservationDto request,
      CancellationToken ct
   ) {
      var result = await _useCases.CreateAsync(
         customerId: request.CustomerId,
         carCategory: request.CarCategory,
         start: request.Start,
         end: request.End,
         id: request.Id,
         ct: ct
      );

      return this.CreatedAt(
         routeName: nameof(GetById),
         routeValues: new {
            id = result.IsSuccess ? result.Value : Guid.Empty
            // version = HttpContext.GetRequestedApiVersion()?.ToString()
         },
         result: result,
         logger: _logger,
         context: $"{nameof(ReservationsController)}.{nameof(Create)}",
         args: request
      );
   }

   /// <summary>
   /// Changes the reservation period.
   /// </summary>
   [HttpPut("booking/reservations/{id:guid}/period", Name = nameof(ChangePeriod))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public async Task<IActionResult> ChangePeriod(
      Guid id,
      [FromBody] ChangePeriodDto request,
      CancellationToken ct
   ) {
      var result = await _useCases.ChangePeriodAsync(
         reservationId: id,
         newStart: request.NewStart,
         newEnd: request.NewEnd,
         ct: ct
      );

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(ReservationsController)}.{nameof(ChangePeriod)}",
         args: new {
            ReservationId = id,
            request
            // Version = HttpContext.GetRequestedApiVersion()?.ToString()
         }
      );
   }

   /// <summary>
   /// Confirms a reservation.
   /// </summary>
   [HttpPost("booking/reservations/{id:guid}/confirm", Name = nameof(Confirm))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public async Task<IActionResult> Confirm(Guid id, CancellationToken ct) {

      var result = await _useCases.ConfirmAsync(id, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(ReservationsController)}.{nameof(Confirm)}",
         args: new {
            ReservationId = id
            // Version = HttpContext.GetRequestedApiVersion()?.ToString()
         }
      );
   }

   /// <summary>
   /// Cancels a reservation.
   /// </summary>
   [HttpPost("booking/reservations/{id:guid}/cancel", Name = nameof(Cancel))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) {

      var result = await _useCases.CancelAsync(id, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(ReservationsController)}.{nameof(Cancel)}",
         args: new {
            ReservationId = id
            // Version = HttpContext.GetRequestedApiVersion()?.ToString()
         }
      );
   }

   /// <summary>
   /// Expires eligible reservations (batch operation).
   /// </summary>
   [HttpPost("booking/reservations/expire", Name = nameof(Expire))]
   [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
   public async Task<IActionResult> Expire(CancellationToken ct) {

      var result = await _useCases.ExpireAsync(ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(ReservationsController)}.{nameof(Expire)}"
         // args: new { Version = HttpContext.GetRequestedApiVersion()?.ToString() }
      );
   }
}
