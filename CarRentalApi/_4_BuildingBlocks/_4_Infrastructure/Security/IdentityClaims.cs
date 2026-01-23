namespace CarRentalApi._4_BuildingBlocks._4_Infrastructure.Security;

/// <summary>
/// Well-known claim names used by the Identity Provider (OIDC/OAuth).
/// Acts as a shared contract between IdP and APIs.
/// </summary>
public static class IdentityClaims {
   public const string Subject = "sub";
   public const string Email = "email";
   public const string CreatedAt = "created_at";
   public const string AdminRights = "admin_rights";
}
