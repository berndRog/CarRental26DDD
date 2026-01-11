// using CarRentalApi.BuildingBlocks;
// using CarRentalApi.Modules.Cars.Application;
// using CarRentalApi.Modules.Customers.Application.Contracts;
// using CarRentalApi.Modules.Customers.Application.contracts.Dto;
// using CarRentalApi.Modules.Customers.Application.Contracts.Dto;
// using CarRentalApi.Modules.Customers.Application.ReadModel;
// using Microsoft.AspNetCore.Mvc;
// namespace CarRentalApi.Modules.Customers.Presentation.Controllers;
//
// [Route("carrentalapi/v1/")]
// [ApiController]
// [Consumes("application/json")] //default
// [Produces("application/json")] //default
//
// public sealed class CustomersController(
//    ICustomerReadModel _customerReadApi,
//    ICustomerUseCases _customerUseCases,
//    ILogger<CustomersController> _logger
// ) : ControllerBase {
//
//
//    [HttpGet("customers/{id:guid}", Name = "GetCustomerById")]
//    [EndpointSummary("Get a customer by ReservationId")]
//    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
//    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
//    public async Task<ActionResult<CustomerDto>> GetCustomerById(
//       [FromRoute] Guid id,
//       CancellationToken ct
//    ) {
//       var result = await _customerReadApi.FindByIdAsync(id, ct);
//       return this.ToActionResult<CustomerDto>(
//          result,
//          _logger,
//          context: "GET /customers/{id}",
//          args: new { id }
//       );
//    }
//
//    [HttpGet("customers/email/{email}")]
//    [EndpointSummary("Get a customer by email")]
//    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
//    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
//    public async Task<ActionResult<CustomerDto>> GetCustomerByEmail(
//       [FromRoute] string email,
//       CancellationToken ct
//    ) {
//       var result = await _customerReadApi.FindByEmailAsync(email, ct);
//       return this.ToActionResult<CustomerDto>(
//          result,
//          _logger,
//          context: "GET /customers/email/{email}",
//          args: new { email }
//       );
//    }
//
//    [HttpGet("customers/name")]
//    [EndpointSummary("Get customers by name")]
//    [ProducesResponseType<IReadOnlyList<CustomerDto>>(StatusCodes.Status200OK)]
//    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetCustomersByName(
//       [FromQuery] string firstName,
//       [FromQuery] string lastName,
//       CancellationToken ct
//    ) {
//       var result = await _customerReadApi.FindByNameAsync(firstName, lastName, ct);
//       return this.ToActionResult<IReadOnlyList<CustomerDto>>(
//          result,
//          _logger,
//          context: "GET /customers/name",
//          args: new { firstName, lastName }
//       );
//    }
//
//    [HttpGet("customers")]
//    [EndpointSummary("Filter customers")]
//    [ProducesResponseType<IReadOnlyList<CustomerDto>>(StatusCodes.Status200OK)]
//    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> FilterCustomers(
//       [FromQuery] CustomerFilter filter,
//       CancellationToken ct
//    ) {
//       var result = await _customerReadApi.FilterAsync(filter, ct);
//       return this.ToActionResult<IReadOnlyList<CustomerDto>>(
//          result,
//          _logger,
//          context: "GET /customers",
//          args: filter
//       );
//    }
//    
//    [HttpPost("customers")]
//    [EndpointSummary("Create a new customer")]
//    [ProducesResponseType(StatusCodes.Status201Created)]
//    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
//    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
//    [Consumes("application/json")]
//    public async Task<ActionResult> Create(
//       [FromBody] CustomerDto customerDto,
//       CancellationToken ct
//    ) {
//       var result = await _customerUseCases.CreateAsync(
//          customerDto.FirstName,
//          customerDto.LastName,
//          customerDto.Email,
//          customerDto.CreatedAt,
//          customerDto.Street,
//          customerDto.PostalCode,
//          customerDto.City,
//          customerDto.Id,
//          ct
//       );
//
//       return this.CreatedAt(
//          routeName: "GetCustomerById",
//          routeValues: new { id = result.Value?.Id },
//          result: result,
//          logger: _logger,
//          context: "POST /customers",
//          args: new { customerId = result.Value?.Id, customerDto.Email }
//       );
//    }
// }
//
//    
//
