using CarRentalApi._2_Modules.Customers._1_Ports.Inbound;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.ReadModels;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;
using CarRentalApi._2_Modules.Customers._2_Application.UseCases;
using CarRentalApi.BuildingBlocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CarRentalApi._2_Modules.Customers._1_Presentation.Controllers;

[ApiController]
[Route("api")]
public sealed class CustomersController(
   ICustomerReadModel _readModel,
   CustomerUcProvisioned _ucProvisioned,
   CustomerUcProfile _ucUpdateProfile,
   ILogger<CustomersController> _logger
) : ControllerBase {

   // ------------------------------------------------------------------
   // SELF-SERVICE (logged-in user)
   // ------------------------------------------------------------------
   [HttpPost("customers/provisioned")]
   [Authorize]
   [EndpointSummary("Provision customer on first login (idempotent)")]
   [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<Guid>> Provisioned(CancellationToken ct) {
      var result = await _ucProvisioned.ExecuteAsync(ct);
      
      return this.ToActionResult<Guid>(
         result,
         _logger,
         context: "POST /customers/provisioned",
         args: new { }
      );
   }

   [HttpGet("customers/profile")]
   [Authorize]
   [EndpointSummary("Get my customer profile (requires provisioning)")]
   [ProducesResponseType<CustomerProfileDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<CustomerProfileDto>> GetMyProfile(CancellationToken ct) {
      var dto = await _readModel.FindProfileAsync(ct);
      if (dto is null)
         return NotFound(new ProblemDetails { Title = "Customer not provisioned" });

      return Ok(dto);
   }

   [HttpPut("customers/profile")]
   [Authorize]
   [EndpointSummary("Update my customer profile (requires provisioning)")]
   [ProducesResponseType<CustomerProfileDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   public async Task<ActionResult<CustomerProfileDto>> UpdateMyProfile(
      [FromBody] CustomerProfileDto dto,
      CancellationToken ct
   ) {
      var result = await _ucUpdateProfile.ExecuteAsync(dto, ct);
      return this.ToActionResult<CustomerProfileDto>(
         result,
         _logger,
         context: "PUT /customers/profile",
         args: dto
      );
   }

   // ------------------------------------------------------------------
   // ADMIN/STAFF READ API (customer directory)
   // ------------------------------------------------------------------
   // Empfehlung:
   // - Entweder: [Authorize(Policy="EmployeesOnly")] hier drüber
   // - oder: im ReadApi/UseCase via _identityGateway.AdminRights prüfen
   //
   // Ich setze hier MINIMAL [Authorize] (Token nötig) und du kannst
   // danach auf Policy hochziehen.
   // ------------------------------------------------------------------

   [HttpGet("customers/{id:guid}", Name = "GetCustomerById")]
   [Authorize] // später ggf. Policy="EmployeesOnly"
   [EndpointSummary("Get a customer by ReservationId")]
   [ProducesResponseType<CustomerDetailDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CustomerDetailDto>> GetCustomerById(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await _readModel.FindByIdAsync(id, ct);
      return this.ToActionResult<CustomerDetailDto>(
         result,
         _logger,
         context: "GET /customers/{id}",
         args: new { id }
      );
   }

   [HttpGet("customers/email/{email}")]
   [Authorize] // später ggf. Policy="EmployeesOnly"
   [EndpointSummary("Get a customer by email")]
   [ProducesResponseType<CustomerDetailDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CustomerDetailDto>> GetCustomerByEmail(
      [FromRoute] string email,
      CancellationToken ct
   ) {
      var result = await _readModel.FindByEmailAsync(email, ct);
      return this.ToActionResult<CustomerDetailDto>(
         result,
         _logger,
         context: "GET /customers/email/{email}",
         args: new { email }
      );
   }

   [HttpGet("customers/name")]
   [Authorize] // später ggf. Policy="EmployeesOnly"
   [EndpointSummary("Get customers by name")]
   [ProducesResponseType<IReadOnlyList<CustomerDetailDto>>(StatusCodes.Status200OK)]
   public async Task<ActionResult<IReadOnlyList<CustomerDetailDto>>> GetCustomersByName(
      [FromQuery] string firstName,
      [FromQuery] string lastName,
      CancellationToken ct
   ) {
      var result = await _readModel.SelectByNameAsync(firstName, lastName, ct);
      return this.ToActionResult<IReadOnlyList<CustomerDetailDto>>(
         result,
         _logger,
         context: "GET /customers/name",
         args: new { firstName, lastName }
      );
   }

   // // Optional: Filter
   // [HttpGet("customers")]
   // [Authorize] // später ggf. Policy="EmployeesOnly"
   // [EndpointSummary("Filter customers")]
   // [ProducesResponseType<IReadOnlyList<CustomerListItemDto>>(StatusCodes.Status200OK)]
   // public async Task<ActionResult<IReadOnlyList<CustomerListItemDto>>> FilterCustomers(
   //     [FromQuery] CustomerSearchFilter filter,
   //     CancellationToken ct
   // ) {
   //     var result = await _readModel.FilterAsync(filter, ct);
   //     return this.ToActionResult<IReadOnlyList<CustomerDto>>(
   //        result,
   //        _logger,
   //        context: "GET /customers",
   //        args: filter
   //     ); 
   // }
}


/*
[Route("carrentalapi/v1/")]
[ApiController]
[Consumes("application/json")] //default
[Produces("application/json")] //default

public sealed class CustomersController(
   ICustomerReadModel _customerReadApi,
   ICustomerUseCases _customerUseCases,
   ILogger<CustomersController> _logger
) : ControllerBase {


   [HttpGet("customers/{id:guid}", Name = "GetCustomerById")]
   [EndpointSummary("Get a customer by ReservationId")]
   [ProducesResponseType<CustomerDetailDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CustomerDetailDto>> GetCustomerById(
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      var result = await _customerReadApi.FindByIdAsync(id, ct);
      return this.ToActionResult<CustomerDetailDto>(
         result,
         _logger,
         context: "GET /customers/{id}",
         args: new { id }
      );
   }

   [HttpGet("customers/email/{email}")]
   [EndpointSummary("Get a customer by email")]
   [ProducesResponseType<CustomerDetailDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<CustomerDetailDto>> GetCustomerByEmail(
      [FromRoute] string email,
      CancellationToken ct
   ) {
      var result = await _customerReadApi.FindByEmailAsync(email, ct);
      return this.ToActionResult<CustomerDetailDto>(
         result,
         _logger,
         context: "GET /customers/email/{email}",
         args: new { email }
      );
   }

   [HttpGet("customers/name")]
   [EndpointSummary("Get customers by name")]
   [ProducesResponseType<IReadOnlyList<CustomerDetailDto>>(StatusCodes.Status200OK)]
   public async Task<ActionResult<IReadOnlyList<CustomerDetailDto>>> GetCustomersByName(
      [FromQuery] string firstName,
      [FromQuery] string lastName,
      CancellationToken ct
   ) {
      var result = await _customerReadApi.SelectByNameAsync(firstName, lastName, ct);
      return this.ToActionResult<IReadOnlyList<CustomerDetailDto>>(
         result,
         _logger,
         context: "GET /customers/name",
         args: new { firstName, lastName }
      );
   }
}
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


*/