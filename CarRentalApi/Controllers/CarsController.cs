using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Application.Contracts.Dto;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApi.Controllers;

[ApiController]
[Route("api/v1/cars")]
public sealed class CarsController(
   ICarUseCases _carUseCases
) : ControllerBase {
   
   [HttpPost]
   public async Task<ActionResult> Create(
      [FromBody] CarDto carDto,
      CancellationToken ct
   ) {
      var result = await _carUseCases.CreateAsync(
         category: carDto.Category,
         manufacturer: carDto.Manufacturer,
         model: carDto.Model, 
         licensePlate: carDto.LicensePlate,          
         id: carDto.Id.ToString(), 
         ct: ct
      );

      // Wenn ihr einen GET-by-id habt, gerne CreatedAtRoute(...) benutzen.
      return CreatedAtAction(nameof(Create), new { id = result.Value?.Id }, result.Value);
   }
}
