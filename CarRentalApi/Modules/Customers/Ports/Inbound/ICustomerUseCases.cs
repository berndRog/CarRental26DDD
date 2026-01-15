using CarRentalApi.BuildingBlocks;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
namespace CarRentalApi.Modules.Cars.Application;

/// <summary>
/// Application use cases for the Customers bounded context.
///
/// Purpose:
/// - Defines all state-changing operations related to customers
/// - Serves as the inbound application boundary (port)
/// - Is called by API controllers or other application services
///
/// Scope:
/// - Create new customers
/// - Change customer lifecycle state (e.g. block customer)
///
/// Architectural intent:
/// - Part of the Application Layer (Inbound Port)
/// - Exposes COMMAND use cases (write side)
/// - Hides concrete implementations behind an interface
///
/// Important:
/// - This interface represents write operations only
/// - It must NOT be used for read/query scenarios
/// - It must NOT be implemented in the Domain Layer
///
/// Result policy:
/// - Success:
///   - Returns created aggregates or completes without payload
/// - Failure:
///   - Returns domain-specific errors (validation, conflicts, not found)
/// </summary>
public interface ICustomerUseCases {
   /// <summary>
   /// Creates a new customer.
   ///
   /// Business meaning:
   /// - Registers a new customer in the system
   /// - Stores the customer's contact data
   ///
   /// Preconditions:
   /// - The provided contact data must be valid
   /// - The email address must not already be in use
   ///
   /// Side effects:
   /// - Creates a new Customer aggregate
   /// - Persists it in the database
   ///
   /// Returns:
   /// - Success(<see cref="Customer"/>) containing the created customer
   /// - Invalid if input data is invalid
   /// - Conflict if a customer with the same email already exists
   /// </summary>
   Task<Result<Customer>> CreateAsync(
      string firstName,
      string lastName,
      string email,
      DateTimeOffset createdAt,
      string? street,
      string? postalCode,
      string? city,
      string? id,
      CancellationToken ct
   );

   /// <summary>
   /// Blocks an existing customer.
   ///
   /// Business meaning:
   /// - Prevents the customer from performing further business actions
   /// - The customer remains in the system for auditing purposes
   ///
   /// Preconditions:
   /// - The customer must exist
   /// - The customer must not already be blocked
   ///
   /// Side effects:
   /// - Changes the lifecycle state of the customer
   ///
   /// Returns:
   /// - Success if the customer was successfully blocked
   /// - NotFound if the customer does not exist
   /// - Conflict if the customer is already blocked
   /// </summary>
   Task<Result> BlockAsync(
      Guid id,
      DateTimeOffset blockedAt,
      CancellationToken ct
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist ICustomerUseCases?
 * --------------------------
 * ICustomerUseCases ist der Inbound Port (Eingangsschnittstelle)
 * für den Customers-Bounded-Context.
 *
 * Er definiert alle fachlichen Anwendungsfälle, die den Zustand
 * eines Kunden verändern.
 *
 * Es handelt sich um die WRITE-Seite des Systems.
 *
 *
 * Was ist ICustomerUseCases NICHT?
 * --------------------------------
 * - Kein ReadModel (keine Listen, keine Suchen)
 * - Kein Repository (keine Persistenzdetails)
 * - Kein Domain Service
 * - Kein Aggregate
 *
 *
 * Warum ein Inbound Port?
 * -----------------------
 * - Controller hängen nur von Interfaces ab
 * - Implementierungen können ausgetauscht werden
 * - Klare Trennung zwischen API und Anwendungslogik
 *
 *
 * Fachliche Einordnung der Use Cases:
 * ----------------------------------
 * - Create:
 *   - Ein neuer Kunde wird im System registriert
 *   - Kontaktdaten werden validiert und gespeichert
 *
 * - Block:
 *   - Kunde wird gesperrt
 *   - Weitere fachliche Aktionen (z. B. Buchungen, Mieten)
 *     sind nicht mehr erlaubt
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Fachliche Regeln & Invarianten:
 *   → Customer Aggregate (Domain Layer)
 *
 * - Persistenz:
 *   → CustomerRepository (Infrastructure)
 *
 * - Lesen / Anzeigen:
 *   → ICustomerReadModel oder Read APIs
 *
 *
 * Typische Aufrufer:
 * ------------------
 * - API Controller
 * - Application Services
 *
 * =====================================================================
 */