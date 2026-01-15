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

   // /// <summary>
   // /// Searches rentals using filter, paging and sorting.
   // /// </summary>
   // [HttpGet("rental/rentals", Name = nameof(Search))]
   // [ProducesResponseType(typeof(PagedResult<RentalListItemDto>), StatusCodes.Status200OK)]
   // public async Task<IActionResult> Search(
   //    [FromQuery] RentalSearchFilter filter,
   //    [FromQuery] PageRequest page,
   //    [FromQuery] SortRequest sort,
   //    CancellationToken ct
   // ) {
   //    var result = await _readModel.SearchAsync(filter, page, sort, ct);
   //
   //    return this.ToActionResult(
   //       result,
   //       _logger,
   //       context: $"{nameof(RentalsController)}.{nameof(Search)}",
   //       args: new { filter, page, sort }
   //    );
   // }

   // /// <summary>
   // /// Creates a new rental (draft).
   // /// </summary>
   // [HttpPost("rental/rentals", Name = nameof(Create))]
   // [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
   // public async Task<IActionResult> Create(
   //    [FromBody] RentalPickupDto request,
   //    CancellationToken ct
   // ) {
   //    var result = await _useCases.PickupAsync(
   //       customerId: request.CustomerId,
   //       carId: request.CarId,
   //       start: request.Start,
   //       end: request.End,
   //       id: request.Id,
   //       ct: ct
   //    );
   //
   //    return this.CreatedAt(
   //       routeName: nameof(GetById),
   //       routeValues: new { id = result.IsSuccess ? result.Value : Guid.Empty },
   //       result: result,
   //       logger: _logger,
   //       context: $"{nameof(RentalsController)}.{nameof(Create)}",
   //       args: request
   //    );
   // }

   /// <summary>
   /// Starts a rental.
   /// </summary>
   [HttpPost("rental/rentals", Name = nameof(PickupAsync))]
   [ProducesResponseType(StatusCodes.Status204NoContent)]
   public async Task<IActionResult> PickupAsync(
      Guid id, 
      RentalPickupDto pickupDto,
      CancellationToken ct
   ) {

      var result = await _useCases.PickupAsync(
         reservationId: pickupDto.ReservationId,
         fuelOut: pickupDto.FuelOut,
         kmOut: pickupDto.KmOut,
         ct
      );
      
      return this.CreatedAt(
         routeName: nameof(GetById),
         routeValues: new {
            id = result.IsSuccess ? result.Value : Guid.Empty
            // version = HttpContext.GetRequestedApiVersion()?.ToString()
         },
         result: result,
         logger: _logger,
         context: $"{nameof(RentalsController)}.{nameof(PickupAsync)}",
         args: pickupDto
      );
   }


}
