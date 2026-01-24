using CarRentalApi._2_Modules.Customers._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Customers._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.Domain;
namespace CarRentalApi._2_Modules.Customers._2_Application.UseCases;

public sealed class CustomerUcProvisioned(
   ICustomerRepository _repository,
   IIdentityGateway _identityGateway,
   IUnitOfWork _unitOfWork,
   ILogger<CustomerUcProvisioned> _logger
) {
   public async Task<Result<Guid>> ExecuteAsync(CancellationToken ct) {

      // 1) subject required
      var subjectResult = IdentitySubject.Create(_identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<Guid>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // 2) idempotent lookup - if already exists, return existing id
      var existing = await _repository.FindByIdentitySubjectAsync(subject, ct);
      if (existing is not null)
         return Result<Guid>.Success(existing.Id);

      // 3) email required
      if (string.IsNullOrWhiteSpace(_identityGateway.Email))
         return Result<Guid>.Failure(CustomerErrors.EmailIsRequired);
      var emailResult = Email.Create(_identityGateway.Email!);
      if (emailResult.IsFailure)
         return Result<Guid>.Failure(emailResult.Error);
      var email = emailResult.Value;

      // 4) createdAt at provisioning time on ID-Provider
      var createdAt = _identityGateway.CreatedAt ?? DateTimeOffset.UtcNow;

      // 5) create domain aggregate
      var customerResult = Customer.CreateProvisioned(subject, email, createdAt);
      if (customerResult.IsFailure)
         return Result<Guid>.Failure(customerResult.Error);

      // 6) add to repository + commit via UnitOfWork
      var customer = customerResult.Value;
      _repository.Add(customer);
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Customer provisioned on first login", ct);

      _logger.LogInformation(
         "Customer provisioned subject={sub} customerId={id} savedRows={rows}",
         subject.Value, customer.Id, savedRows);

      return Result<Guid>.Success(customer.Id);
   }
}
