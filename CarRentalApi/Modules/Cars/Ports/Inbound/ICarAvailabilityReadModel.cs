using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
namespace CarRentalApi.Modules.Cars.Ports.Inbound;
/// <summary>
/// Read model / policy to check if a car has overlaps with rentals or reservations
/// for a given period. Used inside the Car aggregate.
///
/// In our domain:
/// - Reservations are by CarCategory (no CarId)
/// - therefore car-specific overlap means: overlaps with Rentals
///   where Rental.ReservationId -> Reservation defines the period.
/// </summary>
public interface ICarAvailabilityReadModel {
   /// <summary>
   /// Returns true if there is any overlapping rental (car-specific) for the car.
   /// </summary>
   Task<bool> HasOverlapAsync(
      Guid carId,
      RentalPeriod period,
      CancellationToken ct
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist ICarAvailabilityReadModel?
 * ----------------------------------
 * ICarAvailabilityReadModel ist ein spezialisiertes ReadModel /
 * eine Query-Schnittstelle zur Verfügbarkeitsprüfung eines
 * konkreten Fahrzeugs (Car).
 *
 * Es beantwortet ausschließlich die Frage:
 * „Gibt es für dieses Fahrzeug Überschneidungen in einem Zeitraum?“
 *
 *
 * Fachlicher Hintergrund:
 * -----------------------
 * In unserem Domänenmodell gilt:
 * - Reservierungen werden auf Basis einer CarCategory erstellt
 *   (keine konkrete CarId)
 * - Ein konkretes Fahrzeug wird erst beim Pick-up zugewiesen
 *
 * Daher bedeutet „car-spezifische Verfügbarkeit“:
 * - Es darf keine zeitliche Überschneidung mit bestehenden Rentals geben
 * - Der relevante Zeitraum stammt indirekt aus der Reservation,
 *   auf die sich das Rental bezieht
 *
 *
 * Was ist ICarAvailabilityReadModel NICHT?
 * ----------------------------------------
 * - Kein Repository
 * - Kein UseCase
 * - Kein Domain Service
 * - Kein Aggregate
 *
 * Insbesondere:
 * - Es ändert keinen Zustand
 * - Es enthält keine Geschäftslogik
 * - Es kennt keine Statusübergänge
 *
 *
 * Warum wird dieses ReadModel im Car-Aggregate verwendet?
 * -------------------------------------------------------
 * - Die Verfügbarkeitsprüfung ist fachlich Teil der Car-Logik
 * - Die benötigten Daten liegen jedoch außerhalb des Aggregates
 *   (Rentals / Reservations)
 *
 * Deshalb:
 * - Das Car-Aggregate fragt über dieses Interface externe Daten ab
 * - Die eigentliche Datenbeschaffung erfolgt außerhalb des Domain Layers
 *
 * → Klassisches DDD-Muster: Domain + Query-Port
 *
 *
 * Abgrenzung zu anderen Konzepten:
 * --------------------------------
 * - Allgemeine Fahrzeugabfragen:
 *   → ICarReadModel
 *
 * - Zustandsänderungen am Fahrzeug:
 *   → ICarUseCases
 *
 * - Fahrzeugzuweisung beim Pick-up:
 *   → RentalUcPickup
 *
 *
 * Typische Implementierungsdetails:
 * ---------------------------------
 * - EF Core mit AsNoTracking()
 * - Abfrage von Rentals nach carId
 * - Join auf Reservation zur Ermittlung des RentalPeriod
 * - Überlappungsprüfung auf Zeiträume
 *
 * Dadurch:
 * - bleibt das Domain Model sauber
 * - keine Abhängigkeit von EF Core im Aggregate
 * - klare Trennung von Fachlogik und Datenzugriff
 *
 * =====================================================================
 */
