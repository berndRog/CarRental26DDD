using System.Security.Claims;
using CarRentalApi.BuildingBlocks.Ports.Outbound;
namespace CarRentalApi.BuildingBlocks.Infrastructure.Security;

public sealed class IdentityGatewayHttpContext(
   IHttpContextAccessor accessor
) : IIdentityGateway {
   private ClaimsPrincipal? User => accessor.HttpContext?.User;

   public string Subject => User?.FindFirstValue(IdentityClaims.Subject) ?? "";

   public string? Email => User?.FindFirstValue(IdentityClaims.Email);

   public DateTimeOffset? CreatedAt {
      get {
         var v = User?.FindFirstValue(IdentityClaims.CreatedAt);
         return DateTimeOffset.TryParse(v, out var dt) ? dt : null;
      }
   }

   public int AdminRights
      => int.TryParse(User?.FindFirstValue(IdentityClaims.AdminRights), out var m) ? m : 0;
}