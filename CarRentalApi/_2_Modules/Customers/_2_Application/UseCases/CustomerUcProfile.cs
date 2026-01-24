using CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;
using CarRentalApi._2_Modules.Customers._2_Application.Mappings;
using CarRentalApi._2_Modules.Customers._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
using CarRentalApi.Domain;
namespace CarRentalApi._2_Modules.Customers._2_Application.UseCases;

public class CustomerUcProfile(
   ICustomerRepository _repository,
   IIdentityGateway _identityGateway,
   IUnitOfWork _unitOfWork,
   ILogger<CustomerUcProfile> _logger
) {
   
   public async Task<Result<CustomerProfileDto>> ExecuteAsync(
      CustomerProfileDto dto,
      CancellationToken ct
   ) {
      // subject from gateway
      var subjectResult = IdentitySubject.Create(_identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<CustomerProfileDto>.Failure(subjectResult.Error);

      var subject = subjectResult.Value;

      // must be provisioned
      var customer = await _repository.FindByIdentitySubjectAsync(subject, ct);
      if (customer is null)
         return Result<CustomerProfileDto>.Failure(CustomerApplicationErrors.NotProvisioned);

      // optional: forbid employees/admins
      if (_identityGateway.AdminRights != 0)
         return Result<CustomerProfileDto>.Failure(
            CustomerApplicationErrors.EmployeesCannotUpdateCustomerProfile);

      // email (VO)
      var emailResult = Email.Create(dto.Email);
      if (emailResult.IsFailure)
         return Result<CustomerProfileDto>.Failure(emailResult.Error);

      // domain update (now includes country)
      var updateResult = customer.UpdateProfile(
         dto.FirstName,
         dto.LastName,
         emailResult.Value,
         dto.Street,
         dto.PostalCode,
         dto.City,
         dto.Country
      );
      if (updateResult.IsFailure)
         return Result<CustomerProfileDto>.Failure(updateResult.Error);

      await _unitOfWork.SaveAllChangesAsync("Customer profile updated", ct);

      return Result<CustomerProfileDto>.Success(customer.ToCustomerProfileDto());
   }
}