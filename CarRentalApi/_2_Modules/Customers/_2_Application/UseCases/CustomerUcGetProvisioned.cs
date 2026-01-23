using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Customers._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.BuildingBlocks;
using CarRentalApi.Domain;
namespace CarRentalApi._2_Modules.Customers._2_Application.UseCases;

public sealed class CustomerUcGetProvisioned(
   ICustomerRepository _repository,
   IIdentityGateway _identityGateway,
   IUnitOfWork _unitOfWork,
   ILogger<CustomerUcGetProvisioned> _logger
) {
   public async Task<Result<Guid>> ExecuteAsync(CancellationToken ct) {
      // 1) Subject
      var resultSub = IdentitySubject.Create(_identityGateway.Subject);
      if (resultSub.IsFailure)
         return Result<Guid>.Failure(resultSub.Error);

      var subject = resultSub.Value!;

      // 2) Idempotent lookup
      var existing = await _repository.FindByIdentitySubjectAsync(subject, ct);
      if (existing is not null)
         return Result<Guid>.Success(existing.Id);

      // 3) Email required for Customer provisioning
      if (string.IsNullOrWhiteSpace(_identityGateway.Email))
         return Result<Guid>.Failure(CustomerErrors.EmailIsRequired);

      var resultEmail = Email.Create(_identityGateway.Email!);
      if (resultEmail.IsFailure)
         return Result<Guid>.Failure(resultEmail.Error);

      var email = resultEmail.Value!;

      // 4) CreatedAt
      var createdAt = _identityGateway.CreatedAt ?? DateTimeOffset.UtcNow;

      var resultCustomer = Customer.CreateProvisioned(subject, email, createdAt);
      if (resultCustomer.IsFailure)
         return Result<Guid>.Failure(resultCustomer.Error);
      
      // 5) Persist (sync Add) + Commit (async)
      var customer = resultCustomer.Value!;
      _repository.Add(customer);

      var savedRows = await _unitOfWork.SaveAllChangesAsync("Customer provisioned on first login", ct);

      _logger.LogInformation(
         "Customer provisioned subject={sub} customerId={id} savedRows={rows}",
         _identityGateway.Subject, customer.Id, savedRows);

      return Result<Guid>.Success(customer.Id);
   }
}

