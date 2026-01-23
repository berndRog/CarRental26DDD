namespace CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;

public interface IIdentityGateway {
   string Subject { get; }              // OIDC: "sub"
   string? Email { get; }               // OIDC: "email" (optional)
   DateTimeOffset? CreatedAt { get; }   // optional claim
   int AdminRights { get; }             // bitmask claim "admin_rights" (0 for customers)
}
