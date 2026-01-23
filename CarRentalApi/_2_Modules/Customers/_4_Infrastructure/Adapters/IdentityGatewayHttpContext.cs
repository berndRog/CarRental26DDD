using System.Security.Claims;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
namespace CarRentalApi.Modules.Customers._4_Infrastructure.Adapters;

public sealed class IdentityGatewayHttpContext(
   IHttpContextAccessor accessor
) : IIdentityGateway {
   public string Subject =>
      accessor.HttpContext?.User.FindFirstValue("sub") ?? "";

   public string Email =>
      accessor.HttpContext?.User.FindFirstValue("email") ?? "";

   public DateTimeOffset? CreatedAt {
      get {
         var v = accessor.HttpContext?.User.FindFirstValue("created_at");
         return DateTimeOffset.TryParse(v, out var dt) ? dt : null;
      }
   }
   public int AdminRights { get; }
}