using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
namespace CarRentalApiTest.Modules.Customers.Application.UseCases;

public sealed class FakeIdentityGateway : IIdentityGateway {
   public string Subject { get; }
   public string Username { get; }
   public DateTimeOffset CreatedAt { get; }
   public int AdminRights { get; }
   
   public FakeIdentityGateway(
      string subject,
      string username,
      DateTimeOffset createdAt,
      int adminRights
   ) {
      Subject = subject;
      Username = username;
      CreatedAt = createdAt;
      AdminRights = adminRights;
   }
}
   
