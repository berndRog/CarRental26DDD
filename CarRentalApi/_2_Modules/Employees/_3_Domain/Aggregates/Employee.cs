using CarRentalApi._2_Modules.Employees._3_Domain.Enums;
using CarRentalApi._2_Modules.Employees._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks._3_Domain.Entities;
using CarRentalApi._4_BuildingBlocks._3_Domain.ValueObjects;
using CarRentalApi._4_BuildingBlocks.Domain.ValueObjects;
using CarRentalApi.BuildingBlocks;
namespace CarRentalApi._2_Modules.Employees._3_Domain.Aggregates;

/// <summary>
/// Employee aggregate root.
///
/// Represents an employee of the organization and defines
/// all domain rules related to employee lifecycle and administration.
///
/// Responsibilities:
/// - Holds identity and personal data (via Person base class)
/// - Manages administrative rights
/// - Controls activation and deactivation lifecycle
///
/// Invariants:
/// - Personnel number must be present
/// - Creation timestamp must be defined
/// - Admin rights must only contain allowed flag values
///
/// Notes:
/// - This aggregate contains no persistence or application logic
/// - All state changes are enforced via domain methods
/// </summary>
public sealed class Employee : Entity<Guid> {
   
   public string FirstName { get; private set; } = string.Empty;
   public string LastName  { get; private set; } = string.Empty;
   public Email Email     { get; private set; } = default!;
   public Phone? Phone { get; private set; } = null;
   
   public string PersonnelNumber { get; private set; } = string.Empty;
   public AdminRights AdminRights { get; private set; } = AdminRights.ViewReports;
   public bool IsAdmin => AdminRights != AdminRights.None;
   public bool IsActive { get; private set; }
   public DateTimeOffset CreatedAt { get; private set; }
   public DateTimeOffset? DeactivatedAt { get; private set; }

   // EF Core constructor
   private Employee() { }

   // Domain constructor
   private Employee(
      Guid id,
      string firstName,
      string lastName,
      Email email,
      Phone? phone,
      string personnelNumber,
      AdminRights adminRights,
      DateTimeOffset createdAt,
      Address? address = null
   ) {
      Id = id;
      FirstName = firstName;
      LastName  = lastName;
      Email     = email;
      Phone = phone;
      PersonnelNumber = personnelNumber;
      AdminRights = adminRights;
      IsActive = true;
      CreatedAt = createdAt;
   }

   // ---------- Factory (Result-based) ----------
   /// </summary>
   public static Result<Employee> Create(
      string firstName,
      string lastName,
      string emailString,
      string phoneString,
      string personnelNumber,
      AdminRights adminRights = AdminRights.None,
      DateTimeOffset createdAt = default,
      string? id = null,
      Address? address = null
   ) {
      // Normalize input early
      firstName = firstName?.Trim() ?? string.Empty;
      lastName = lastName?.Trim() ?? string.Empty;
      emailString = emailString?.Trim() ?? string.Empty;
      phoneString = phoneString?.Trim() ?? string.Empty;
      personnelNumber = personnelNumber?.Trim() ?? string.Empty;

      if (string.IsNullOrWhiteSpace(firstName))
         return Result<Employee>.Failure(EmployeeErrors.FirstNameIsRequired);
      if (firstName.Length is < 2 or > 100)
         return Result<Employee>.Failure(EmployeeErrors.InvalidFirstName);
      
      if (string.IsNullOrWhiteSpace(lastName))
         return Result<Employee>.Failure(EmployeeErrors.LastNameIsRequired);
      if (lastName.Length is < 2 or > 100)
         return Result<Employee>.Failure(EmployeeErrors.InvalidFirstName);

      if (string.IsNullOrWhiteSpace(emailString))
         return Result<Employee>.Failure(EmployeeErrors.EmailIsRequired);
      var resultEmail = Email.Create(emailString);
      if(!resultEmail.IsFailure) 
         return Result<Employee>.Failure(EmployeeErrors.InvalidEmail);
      var email = resultEmail.Value!;
      
      Phone? phone = null;
      if (!string.IsNullOrWhiteSpace(phoneString)) {
         var resultPhone = Phone.Create(phoneString);
         if (!resultPhone.IsFailure)
            return Result<Employee>.Failure(resultPhone.Error);
         phone = resultPhone.Value!;
      }
      
      if (string.IsNullOrWhiteSpace(personnelNumber))
         return Result<Employee>.Failure(EmployeeErrors.PersonnelNumberIsRequired);

      if (createdAt == default)
         return Result<Employee>.Failure(EmployeeErrors.CreatedAtIsRequired);

      var result = EntityId.Resolve(id, EmployeeErrors.InvalidId);
      if (result.IsFailure)
         return Result<Employee>.Failure(result.Error);

      var employee = new Employee(
         result.Value,
         firstName,
         lastName,
         email,
         phone,
         personnelNumber,
         adminRights,
         createdAt,
         address
      );
      return Result<Employee>.Success(employee);
   }

   // ---------- Domain operations ----------

   /// <summary>
   /// Replaces the administrative rights of the employee.
   ///
   /// Semantics:
   /// - The provided rights replace the previous rights completely
   /// - Partial add/remove operations are intentionally not supported
   ///
   /// Returns:
   /// - Success if the rights are valid and applied
   /// - Failure if the bitmask contains unsupported flags
   /// </summary>
   public Result SetAdminRights(AdminRights adminRights) {

      // Validate allowed bits
      if (!Enum.IsDefined(typeof(AdminRights), adminRights))
         return Result.Failure(EmployeeErrors.InvalidAdminRightsBitmask);

      AdminRights = adminRights;
      return Result.Success();
   }

   /// <summary>
   /// Deactivates the employee.
   ///
   /// Business rules:
   /// - An employee can only be deactivated once
   ///
   /// Side effects:
   /// - Sets IsActive to false
   /// - Records the deactivation timestamp
   /// </summary>
   public Result Deactivate(DateTimeOffset deactivatedAt) {
      if (!IsActive)
         return Result.Failure(EmployeeErrors.AlreadyDeactivated);

      IsActive = false;
      DeactivatedAt = deactivatedAt;
      return Result.Success();
   }
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist das Employee-Aggregat?
 * ------------------------------
 * Employee ist das Aggregate Root des Employees-Bounded-Contexts.
 *
 * Es modelliert:
 * - Identität und Personendaten (über die Basisklasse Person)
 * - administrative Berechtigungen (AdminRights)
 * - den fachlichen Lebenszyklus eines Mitarbeiters
 *
 *
 * Warum eine Result-basierte Factory?
 * -----------------------------------
 * Die statische Create-Methode stellt sicher, dass:
 * - alle fachlichen Invarianten beim Erzeugen geprüft werden
 * - kein ungültiges Employee-Objekt entstehen kann
 * - Fehler eindeutig als DomainErrors zurückgegeben werden
 *
 *
 * Wie werden AdminRights behandelt?
 * ---------------------------------
 * AdminRights werden IMMER als vollständiger Satz gesetzt.
 *
 * Das bedeutet:
 * - Der neue Wert ersetzt den bisherigen komplett
 * - Es gibt kein inkrementelles Hinzufügen oder Entfernen
 *
 * Vorteil:
 * - deterministischer, sicherer Rechtezustand
 * - einfache Autorisierungslogik
 * - keine schleichenden Berechtigungsreste
 *
 *
 * Aktiv / Inaktiv:
 * ----------------
 * Ein Employee ist entweder aktiv oder deaktiviert.
 * Die Deaktivierung ist:
 * - ein fachlicher Zustand
 * - irreversibel ohne expliziten Reaktivierungs-UseCase
 *
 *
 * Abgrenzung:
 * -----------
 * - Persistenz (EF Core): Infrastructure Layer
 * - Orchestrierung (Create, Deactivate, SetRights):
 *   Application UseCases
 * - Lesen / Suchen / Listen:
 *   EmployeeReadModel
 *
 *
 * Merksatz:
 * ---------
 * Aggregate schützen ihre Invarianten selbst.
 * UseCases orchestrieren – Aggregate entscheiden.
 *
 * =====================================================================
 */
