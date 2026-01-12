using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.ReadModel;
using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Application.Contracts.Dto;
using CarRentalApi.Modules.Cars.Application.ReadModel;
using CarRentalApi.Modules.Cars.Application.ReadModel.Dto;
using CarRentalApi.Modules.Cars.Application.UseCases.Dto;
using CarRentalApi.Modules.Cars.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;
namespace CarRentalApi.Controllers;

[Route("carrentalapi/v1")]
[ApiController]
[Consumes("application/json")] //default
[Produces("application/json")] //default
public sealed class CarsController(
   ICarUseCases _carUseCases,
   ICarReadModel _readModel,
   ILogger<CarsController> _logger
) : ControllerBase {
   

   /// <summary>
   /// Returns a single CarDetail.
   /// </summary>
   [HttpGet("{carId:guid}", Name = Routes.Cars.GetById)]
   [ProducesResponseType(typeof(CarDetails), StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
   public async Task<ActionResult> GetByIdAsync(
      Guid carId,
      CancellationToken ct
   ) {
      var result = await _readModel.FindByIdAsync(carId, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: "CarsController.GetById",
         args: new { carId }
      );
   }

   /// <summary>
   /// Searches cars with filter + paging + sorting.
   /// Example:
   ///   /api/v1/cars?pageNumber=1&pageSize=20&sortBy=licensePlate&direction=Asc&category=Suv&isRetired=false&searchText=H-
   /// </summary>
   [HttpGet(Name = Routes.Cars.Search)]
   [ProducesResponseType(typeof(PagedResult<CarListItem>), StatusCodes.Status200OK)]
   [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
   public async Task<ActionResult> SearchAsync(
      [FromQuery] string? searchText,
      [FromQuery] CarCategory? category,
      [FromQuery] bool? isInMaintenance,
      [FromQuery] bool? isRetired,
      [FromQuery] int pageNumber = 1,
      [FromQuery] int pageSize = 20,
      [FromQuery] string sortBy = CarSortFields.LicensePlate,
      [FromQuery] SortDirection direction = SortDirection.Asc,
      CancellationToken ct = default
   ) {
      var filter = new CarSearchFilter(
         SearchText: searchText,
         Category: category,
         IsInMaintenance: isInMaintenance,
         IsRetired: isRetired
      );

      var page = new PageRequest(pageNumber, pageSize);
      var sort = new SortRequest(sortBy, direction);

      var result = await _readModel.SearchAsync(filter, page, sort, ct);

      return this.ToActionResult(
         result,
         _logger,
         context: "CarsController.Search",
         args: new { searchText, category, isInMaintenance, isRetired, pageNumber, pageSize, sortBy, direction }
      );
   }
   
   
   [HttpPost("cars")]
   public async Task<ActionResult> Create(
      [FromBody] CarDto carDto,
      CancellationToken ct
   ) {
      var result = await _carUseCases.CreateAsync(
         manufacturer: carDto.Manufacturer,
         model: carDto.Model,
         licensePlate: carDto.LicensePlate,
         category: carDto.Category,
         createdAt: carDto.CreatedAt,
         id: carDto.CarId.ToString(),
         ct: ct
      );
      var carId = result.Value!;
      
      return this.CreatedAt(
         Routes.Cars.GetById,
         routeValues: result.IsSuccess
            ? new { carId = carId } 
            : null,
         result: result,
         logger: _logger,
         context: "CarsController.Create",
         args: new { carDto = carDto }
      );
   }
   
   // ---------------------------
   // Routes (names for CreatedAtRoute etc.)
   // ---------------------------
   public static class Routes {
      public static class Cars {
         public const string GetById = "Cars.GetById";
         public const string Search = "Cars.Search";
         public const string ByCategory = "Cars.ByCategory";
         public const string InMaintenance = "Cars.InMaintenance";
         public const string Retired = "Cars.Retired";
      }
   }
}