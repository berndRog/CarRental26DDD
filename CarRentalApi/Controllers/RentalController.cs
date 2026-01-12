// CarRentalApi/Controllers/RentalsController.cs
using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Bookings.Application.UseCases.Dto;
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
   ILogger<RentalsController> _logger
) : ControllerBase {

   /// <summary>
   /// Returns rental details by id.
   /// </summary>
   [HttpGet("rental/rentals/{id:guid}", Name = nameof(GetById))]
   [ProducesResponseType(typeof(RentalDetailsDto), StatusCodes.Status200OK)]
   public async Task<IActionResult> GetById(Guid id, CancellationToken ct) {

      var result = await _readModel.FindByIdAsync(id, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(RentalsController)}.{nameof(GetById)}",
         args: new { RentalId = id }
      );
   }

   /// <summary>
   /// Searches rentals using filter, paging and sorting.
   /// </summary>
   [HttpGet("rental/rentals", Name = nameof(Search))]
   [ProducesResponseType(typeof(PagedResult<RentalListItemDto>), StatusCodes.Status200OK)]
   public async Task<IActionResult> Search(
      [FromQuery] RentalSearchFilter filter,
      [FromQuery] PageRequest page,
      [FromQuery] SortRequest sort,
      CancellationToken ct
   ) {
      var result = await _readModel.SearchAsync(filter, page, sort, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(RentalsController)}.{nameof(Search)}",
         args: new { filter, page, sort }
      );
   }

   /// <summary>
   /// Creates a new rental (draft).
   /// </summary>
   [HttpPost("rental/rentals", Name = nameof(Create))]
   [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
   public async Task<IActionResult> Create(
      [FromBody] RentalPickupDto request,
      CancellationToken ct
   ) {
      var result = await _useCases.PickupAsync(
         customerId: request.CustomerId,
         carId: request.CarId,
         start: request.Start,
         end: request.End,
         id: request.Id,
         ct: ct
      );

      return this.CreatedAt(
         routeName: nameof(GetById),
         routeValues: new { id = result.IsSuccess ? result.Value : Guid.Empty },
         result: result,
         logger: _logger,
         context: $"{nameof(RentalsController)}.{nameof(Create)}",
         args: request
      );
   }

   /// <summary>
   /// Changes the rental period.
   /// </summary>
   [HttpPut("rental/rentals/{id:guid}/period", Name = nameof(ChangePeriod))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public async Task<IActionResult> ChangePeriod(
      Guid id,
      [FromBody] ChangeRentalPeriodDto request,
      CancellationToken ct
   ) {
      var result = await _useCases.ChangePeriodAsync(
         rentalId: id,
         newStart: request.NewStart,
         newEnd: request.NewEnd,
         ct: ct
      );

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(RentalsController)}.{nameof(ChangePeriod)}",
         args: new { RentalId = id, request }
      );
   }

   /// <summary>
   /// Starts a rental.
   /// </summary>
   [HttpPost("rental/rentals/{id:guid}/start", Name = nameof(Start))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public async Task<IActionResult> Start(Guid id, CancellationToken ct) {

      var result = await _useCases.StartAsync(id, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(RentalsController)}.{nameof(Start)}",
         args: new { RentalId = id }
      );
   }

   /// <summary>
   /// Ends a rental.
   /// </summary>
   [HttpPost("rental/rentals/{id:guid}/end", Name = nameof(End))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public async Task<IActionResult> End(Guid id, CancellationToken ct) {

      var result = await _useCases.EndAsync(id, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(RentalsController)}.{nameof(End)}",
         args: new { RentalId = id }
      );
   }

   /// <summary>
   /// Cancels a rental.
   /// </summary>
   [HttpPost("rental/rentals/{id:guid}/cancel", Name = nameof(Cancel))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) {

      var result = await _useCases.CancelAsync(id, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: $"{nameof(RentalsController)}.{nameof(Cancel)}",
         args: new { RentalId = id }
      );
   }
}
