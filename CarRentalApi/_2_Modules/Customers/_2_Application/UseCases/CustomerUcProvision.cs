using CarRentalApi._2_Modules.Customers._1_Ports.Outbound;
using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Customers._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._3_Domain;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
namespace CarRentalApi._2_Modules.Customers._2_Application.UseCases;

public sealed class CustomerUcProvision(
   IIdentityGateway _identityGateway,
   ICustomerRepository _repository,
   IUnitOfWork _unitOfWork,
   ILogger<CustomerUcProvision> _logger
) {
   public async Task<Result<Guid>> ExecuteAsync(
      string? id, 
      CancellationToken ct
   ) {

      // 1) subject required
      var result = IdentitySubject.Check(_identityGateway.Subject);
      if (result.IsFailure)
         return Result<Guid>.Failure(result.Error);
      var subject = result.Value;

      // 2) idempotent lookup
      var existing = await _repository.FindByIdentitySubjectAsync(subject, false, ct);
      if (existing is not null)
         return Result<Guid>.Success(existing.Id);

      // 3) required identity data (translate missing-claim exceptions)
      string username;
      DateTimeOffset createdAt;
      try {
         username = _identityGateway.Username; // preferred_username
         createdAt = _identityGateway.CreatedAt; // created_at
      }
      catch (InvalidOperationException ex) {
         _logger.LogWarning(ex, "Provisioning failed: required identity claim missing (sub={sub})", subject);
         return Result<Guid>.Failure(CommonErrors.IdentityClaimsMissing);
      }

      // interpret preferred_username as initial email
      var emailResult = EmailAddress.Check(username);
      if (emailResult.IsFailure)
         return Result<Guid>.Failure(emailResult.Error);
      var email = emailResult.Value;
      
      // check uniqueness
      var existingWithEmail = await _repository.FindByEmailAsync(email, ct);
      if (existingWithEmail is not null)
         return Result<Guid>.Failure(CustomerApplicationErrors.EmailAlreadyInUse);
      
      // 4) create aggregate
      var customerResult = Customer.CreateProvisioned(subject, email, createdAt, id);
      if (customerResult.IsFailure)
         return Result<Guid>.Failure(customerResult.Error);

      // 5) add to repository
      var customer = customerResult.Value;
      _repository.Add(customer);

      // 6) persist with unit of work
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Customer provisioned on first login", ct);

      _logger.LogInformation(
         "Customer provisioned subject={sub} customerId={id} savedRows={rows}",
         subject, customer.Id, savedRows
      );
      return Result<Guid>.Success(customer.Id);
   }
}