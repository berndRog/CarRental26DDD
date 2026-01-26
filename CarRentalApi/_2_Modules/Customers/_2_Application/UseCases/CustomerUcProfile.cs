using CarRentalApi._2_Modules.Customers._1_Ports.Outbound;
using CarRentalApi._2_Modules.Customers._2_Application.Dtos.UseCases;
using CarRentalApi._2_Modules.Customers._2_Application.Mappings;
using CarRentalApi._2_Modules.Customers._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
namespace CarRentalApi._2_Modules.Customers._2_Application.UseCases;

public class CustomerUcProfile(
   IIdentityGateway _identityGateway,
   ICustomerRepository _repository,
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

      // override email address (if changed) 
      var email = customer.Email;
      if (!string.Equals(email.Value, dto.EmailString, StringComparison.OrdinalIgnoreCase)) {
         // create new email value object from dto.Email
         var resultDtoEmail = Email.Create(dto.EmailString);
         if (resultDtoEmail.IsFailure)
            return Result<CustomerProfileDto>.Failure(resultDtoEmail.Error);
         // check uniqueness
         var dtoEmail = resultDtoEmail.Value;
         var existingByEmail = await _repository.FindByEmailAsync(dtoEmail, ct);
         if (existingByEmail is not null && existingByEmail.Id != customer.Id)
            return Result<CustomerProfileDto>.Failure(CustomerApplicationErrors.EmailAlreadyInUse);
         // override previous email
         email = dtoEmail;
      }

      // domain update (now includes country)
      var updateResult = customer.UpdateProfile(
         dto.Firstname,
         dto.Lastname,
         email,
         dto.Street,
         dto.PostalCode,
         dto.City,
         dto.Country
      );
      if (updateResult.IsFailure)
         return Result<CustomerProfileDto>.Failure(updateResult.Error);

      // persist changes with unit of work
      var savedRows = await _unitOfWork.SaveAllChangesAsync("Customer profile updated", ct);

      _logger.LogInformation(
         "Customer profile subject={sub} customerId={id} savedRows={rows}",
         subject.Value, customer.Id, savedRows
      );
      
      return Result<CustomerProfileDto>.Success(customer.ToCustomerProfileDto());
   }
}