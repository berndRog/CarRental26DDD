using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
namespace CarRentalApi.Modules.Bookings.Domain.Policies;

/// <summary>
/// Defines a policy for detecting capacity conflicts
/// when confirming or modifying a reservation.
///
/// This policy evaluates whether a reservation for a given
/// car category and rental period can be accepted
/// with respect to current fleet capacity and existing bookings.
///
/// IMPORTANT:
/// - This is a domain-facing abstraction.
/// - The interface defines *what* is checked, not *how*.
/// - The concrete implementation may require database access
///   and therefore belongs to the Infrastructure layer.
/// </summary>
public interface IReservationConflictPolicy {

   /// <summary>
   /// Checks whether a reservation would cause a capacity conflict
   /// for the given car category and rental period.
   ///
   /// The check considers:
   /// - the total number of cars in the given category
   /// - existing confirmed reservations that overlap
   ///   with the specified rental period
   ///
   /// The reservation identified by <paramref name="ignoreReservationId"/>
   /// is excluded from the conflict check. This is required when
   /// changing the rental period of an existing reservation.
   /// </summary>
   /// <param name="carCategory">
   /// The car category requested by the reservation.
   /// </param>
   /// <param name="period">
   /// The rental period to be checked for availability.
   /// </param>
   /// <param name="ignoreReservationId">
   /// The identifier of a reservation that should be ignored
   /// during the conflict check (e.g. the current reservation
   /// when modifying its rental period).
   /// </param>
   /// <param name="ct">
   /// Cancellation token for the asynchronous operation.
   /// </param>
   /// <returns>
   /// A <see cref="ReservationConflict"/> value describing
   /// whether a capacity conflict exists and of which type.
   /// </returns>
   Task<ReservationConflict> CheckAsync(
      CarCategory carCategory,
      RentalPeriod period,
      Guid ignoreReservationId,
      CancellationToken ct
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Hinweise
 * =====================================================================
 *
 * Was macht diese Policy?
 * -----------------------
 * Die IReservationConflictPolicy kapselt die fachliche Entscheidung,
 * ob eine Reservierung für eine bestimmte Fahrzeugkategorie
 * in einem gegebenen Mietzeitraum bestätigt werden darf.
 *
 * Sie beantwortet ausschließlich die Frage:
 * "Ist in der gewünschten Kategorie im angegebenen Zeitraum
 * noch ausreichend Kapazität vorhanden?"
 *
 *
 * Fachlicher Hintergrund:
 * ----------------------
 * - Reservierungen blockieren keine konkrete Fahrzeug-ID,
 *   sondern Kapazität innerhalb einer Fahrzeugkategorie.
 * - Ein Konflikt liegt vor, wenn:
 *   1) es in der Kategorie keine Fahrzeuge gibt
 *      (ReservationConflict.NoCategoryCapacity)
 *   2) zwar Fahrzeuge existieren, diese aber im gewünschten
 *      Zeitraum bereits vollständig durch bestätigte
 *      Reservierungen belegt sind
 *      (ReservationConflict.OverCapacity)
 *
 *
 * Warum ist das eine Policy und kein Domain Service?
 * --------------------------------------------------
 * - Die Entscheidung benötigt Daten aus mehreren Aggregates
 *   (Cars, Reservations).
 * - Solche abteilungsübergreifenden Abfragen können nicht
 *   als Invariant eines einzelnen Aggregates modelliert werden.
 * - Deshalb wird die Regel als Policy abstrahiert.
 *
 *
 * Architekturentscheidung:
 * ------------------------
 * - Dieses Interface gehört zur Domain (fachliche Regel).
 * - Die Implementierung greift auf Repositories zu und gehört
 *   daher in den Infrastructure Layer.
 * - Der Application Layer nutzt die Policy, um Use Cases wie
 *   "Confirm Reservation" oder "Change Rental Period"
 *   fachlich korrekt abzusichern.
 *
 *
 * Bedeutung von ignoreReservationId:
 * ---------------------------------
 * - Ermöglicht die Wiederverwendung der gleichen Policy
 *   für unterschiedliche Use Cases.
 * - Verhindert, dass sich eine bestehende Reservierung
 *   bei einer Zeitraumänderung selbst blockiert.
 *
 *
 * Abgrenzung:
 * -----------
 * - Diese Policy ändert keinen Zustand.
 * - Sie berechnet keine Preise.
 * - Sie führt keine Statusübergänge aus.
 *
 * =====================================================================
 */
