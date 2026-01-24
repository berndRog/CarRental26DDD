using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._3_Domain.Errors;
namespace CarRentalApi._4_BuildingBlocks._2_Application.Security;

public static class AuthGuards {

   /// <summary>
   /// Ensures the current identity represents an employee/admin.
   /// Used for directory-style read access (e.g. FindById, FindByEmail).
   /// </summary>
   public static Result EnsureEmployee(IIdentityGateway identity) {
      return identity.AdminRights != 0
         ? Result.Success()
         : Result.Failure(DomainErrors.Forbidden);
   }

   /// <summary>
   /// Ensures the current identity represents a customer (self-service).
   /// Used for profile access and self-owned operations.
   /// </summary>
   public static Result EnsureCustomer(IIdentityGateway identity) {
      return identity.AdminRights == 0
         ? Result.Success()
         : Result.Failure(DomainErrors.Forbidden);
   }
}