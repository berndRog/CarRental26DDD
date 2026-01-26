using System.Security.Claims;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
namespace CarRentalApi._4_BuildingBlocks._4_Infrastructure.Security;

/// <summary>
/// Reads identity-related claims of the currently authenticated user
/// from the ASP.NET Core HttpContext and exposes them via IIdentityGateway.
///
/// This class is the ONLY place where HTTP / Claims / OIDC details
/// are accessed directly.
/// </summary>
public sealed class IdentityGatewayHttpContext(
   IHttpContextAccessor accessor
) : IIdentityGateway {

   // Access to the current ClaimsPrincipal (may be null outside HTTP requests)
   private ClaimsPrincipal? User => accessor.HttpContext?.User;

   /// <summary>
   /// OIDC subject ("sub").
   /// This is the stable, opaque identifier issued by the Identity Provider.
   /// Never interpreted, never parsed – only forwarded into the domain
   /// as IdentitySubject.
   /// </summary>
   public string Subject =>
      accessor.HttpContext?.User.FindFirstValue("sub")
      ?? accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? throw new InvalidOperationException("Missing claim: sub");

   /// <summary>
   /// Preferred Username interpreted as initial Email provided by the IdP.
   /// Used ONLY for initial provisioning of a Customer.
   /// </summary>
   public string Username =>
      User?.FindFirstValue(IdentityClaims.PreferredUsername)
      ?? throw new InvalidOperationException("Missing claim: preferred_username");


   /// <summary>
   /// Optional creation timestamp of the identity.
   /// If present, it can be used as the Customer.CreatedAt value
   /// during provisioning.
   /// </summary>
   public DateTimeOffset CreatedAt {
      get {
         var v = User?.FindFirstValue(IdentityClaims.CreatedAt);
         return DateTimeOffset.TryParse(v, out var dt) 
            ? dt 
            : throw new InvalidOperationException("Missing claim: created_at");
         
      }
   }

   /// <summary>
   /// Bitmask defining administrative rights.
   /// 0 = regular customer
   /// != 0 = employee / admin
   ///
   /// The interpretation of this value is a BUSINESS decision
   /// and must happen in Use Cases, not here.
   /// </summary>
   public int AdminRights => 
      int.TryParse(User?.FindFirstValue(IdentityClaims.AdminRights), out var adminRights)
         ? adminRights
         : 0;
}

/* ======================================================================
   DIDAKTIK & LERNZIELE
   ======================================================================

   Zweck dieser Klasse
   -------------------
   IdentityGatewayHttpContext kapselt den technischen Zugriff auf
   HTTP-Claims (OIDC / OAuth) und stellt sie als einfache, fachlich
   benannte Properties bereit.

   Wichtig: Diese Klasse gehört NICHT zur Domain.
   Sie ist reine Infrastruktur.

   Zentrale didaktische Aussagen
   -----------------------------
   1. Die Domain kennt KEINE Claims, KEIN HttpContext und KEIN OIDC.
      Sie arbeitet nur mit eigenen Konzepten (z. B. IdentitySubject).

   2. Alle externen Identitätsdaten werden an EINEM Ort gelesen.
      → Keine FindFirstValue-Aufrufe in Controllern oder Use Cases.

   3. Subject ("sub") ist ein externer Bezeichner, kein Domain-Id.
      → Deshalb wird er später in ein Value Object (IdentitySubject)
        überführt.

   4. Email aus dem IdP ist nur ein Startwert.
      → Nach dem Provisioning ist Email eine fachliche Eigenschaft
        des Customer-Aggregats.

   5. AdminRights sind KEINE Security-Policy.
      → Sie sind ein Input für fachliche Entscheidungen
        (z. B. "Employee darf kein Customer-Profil ändern").

   Lernziele für Studierende
   -------------------------
   - Trennung von Authentifizierung (IdP) und Fachdomäne verstehen
   - Vermeidung von "stringly typed" Claims im Domain-Code
   - Saubere Port-/Adapter-Struktur (IdentityGateway als Outbound Port)
   - Klare Verantwortung:
       * Infrastructure liest Claims
       * Application entscheidet fachlich
       * Domain setzt Regeln durch

   Merksatz
   --------
   „Der IdP liefert Identität – Bedeutung entsteht erst im Use Case.“
   ======================================================================
*/
