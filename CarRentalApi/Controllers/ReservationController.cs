/*
using CarRentalApi.BuildingBlocks.Errors;
using CarRentalApi.Modules.Reservations.Api.Dto;
using CarRentalApi.Modules.Reservations.Application.Contracts.Dto;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApi.Modules.Reservations.Api.Controllers;

[ApiController]
[Route("api/v1/reservations")]
public sealed class ReservationsController(
   IReservationReadApi _services,
   IReservationUseCases _useCases,
) : ControllerBase
{
   // -------------------------
   // POST /reservations
   // Create (always Draft)
   // -------------------------
   [HttpPost]
   public async Task<ActionResult<ReservationDto>> CreateAsync(
      [FromBody] CreateReservationDto dto,
      CancellationToken ct
   ) {
      var result = await _useCases.CreateAsync(
         customerId: dto.CustomerId,
         carCategory: dto.Category,
         start: dto.Start,
         end: dto.End,
         id: null,
         ct: ct
      );

      if (result.IsFailure)
         return ToProblem(result.Error);

      var reservation = result.Value!;
      return CreatedAtAction(
         nameof(FindByIdAsync),
         new { reservationId = reservation.Id },
         ReservationMapper.ToDto(reservation)
      );
   }

   // -------------------------
   // GET /reservations/{id}
   // -------------------------
   [HttpGet("{reservationId:guid}")]
   public async Task<ActionResult<ReservationDto>> FindByIdAsync(
      Guid reservationId,
      CancellationToken ct
   ) {
      var dto = await _queries.FindByIdAsync(reservationId, ct);
      return dto is null ? NotFound() : Ok(dto);
   }

   // -------------------------
   // PUT /reservations/{id}/period
   // -------------------------
   [HttpPut("{reservationId:guid}/period")]
   public async Task<ActionResult> ChangePeriodAsync(
      Guid reservationId,
      [FromBody] ChangePeriodDto dto,
      CancellationToken ct
   ) {
      var result = await _useCases.ChangePeriodAsync(
         reservationId,
         dto.Start,
         dto.End,
         ct
      );

      return result.IsFailure ? ToProblem(result.Error) : NoContent();
   }

   // -------------------------
   // POST /reservations/{id}/confirm
   // -------------------------
   [HttpPost("{reservationId:guid}/confirm")]
   public async Task<ActionResult> ConfirmAsync(
      Guid reservationId,
      CancellationToken ct
   ) {
      var result = await _useCases.ConfirmAsync(reservationId, ct);
      return result.IsFailure ? ToProblem(result.Error) : NoContent();
   }

   // -------------------------
   // POST /reservations/{id}/cancel
   // -------------------------
   [HttpPost("{reservationId:guid}/cancel")]
   public async Task<ActionResult> CancelAsync(
      Guid reservationId,
      CancellationToken ct
   ) {
      var result = await _useCases.CancelAsync(reservationId, ct);
      return result.IsFailure ? ToProblem(result.Error) : NoContent();
   }

   // -------------------------
   // POST /reservations/expire
   // -------------------------
   [HttpPost("expire")]
   public async Task<ActionResult<int>> ExpireAsync(CancellationToken ct) {
      var result = await _useCases.ExpireAsync(ct);
      return result.IsFailure ? ToProblem(result.Error) : Ok(result.Value);
   }

   // -------------------------
   // Error → HTTP (zentral)
   // -------------------------
   private ActionResult ToProblem(DomainErrors error) =>
      Problem(
         title: error.Code,
         detail: error.Message,
         statusCode: DomainErrorHttpMapper.ToStatusCode(error)
      );
}
*/
