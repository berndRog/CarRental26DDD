// using System.Security.Claims;
// using CarRentalApi._2_Modules.Customers._1_Ports.Inbound;
// using CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;
// using CarRentalApi._2_Modules.Customers._2_Application.UseCases;
// using CarRentalApi._2_Modules.Customers._3_Domain.Errors;
// using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// namespace CarRentalApi._1_Controllers;
//
// [ApiController]
// [Route("carrentalapi/v1")]
// [Authorize] // Access Token required
// public sealed class ProvisionedController(
//    CustomerUcProvisioned _uc // oder ICustomerProvisioningUseCase
// ) : ControllerBase {
//    // POST /api/provisioned
//    // Idempotent: creates customer on first login, otherwise no-op (returns existing id)
//    [HttpPost("/customers/provisioned")]
//    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    public async Task<ActionResult<Guid>> PostAsync(CancellationToken ct) {
//       var result = await _uc.ExecuteAsync(ct);
//
//       if (result.IsSuccess)
//          return Ok(result.Value);
//
//       // Map your Result error to HTTP
//       // - identity missing/invalid -> 401
//       // - validation/domain errors -> 400
//       // (adapt to your existing error->status mapping helpers if you have them)
//       var err = result.Error;
//
//       if (err == CommonErrors.InvalidIdentitySubject) // or err.Code == ...
//          return Unauthorized(err);
//
//       return BadRequest(err);
//    }
// }
//
//
// [ApiController]
// [Route("api/profile")]
// [Authorize]
// public sealed class ProfileController(
//    ICustomerReadModel _readModel,
//    CustomerUcProfile _ucUpdate // name as in your project
// ) : ControllerBase {
//    // GET /api/profile
//    // Requires: customer already provisioned (POST /api/provisioned)
//    [HttpGet]
//    [ProducesResponseType(typeof(CustomerProfileDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    public async Task<ActionResult<CustomerProfileDto>> GetAsync(CancellationToken ct) {
//       var profile = await _readModel.FindProfileAsync(ct); // uses IIdentityGateway internally
//       if (profile is null)
//          return NotFound("Customer not provisioned.");
//
//       return Ok(profile);
//    }
//
//    // PUT /api/profile
//    [HttpPut]
//    [ProducesResponseType(typeof(CustomerProfileDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType(StatusCodes.Status400BadRequest)]
//    public async Task<ActionResult<CustomerProfileDto>> PutAsync(
//       [FromBody] CustomerProfileDto dto,
//       CancellationToken ct
//    ) {
//       var result = await _ucUpdate.ExecuteAsync(dto, ct); // resolves subject via IIdentityGateway
//
//       if (result.IsSuccess)
//          return Ok(result.Value);
//
//       // If your Result has a dedicated "not found" error, map it here:
//       if (result.Error == CustomerApplicationErrors.NotProvisioned)
//          return NotFound(result.Error);
//
//       return BadRequest(result.Error);
//    }
// }