
namespace CarRentalApi.Modules.Bookings.Domain.Enums;
/// <summary>
/// Contractual fuel level classification at pick-up / return time.
/// Not a physical measurement.
/// </summary>
public enum RentalFuelLevel : int {
   Empty = 0,
   Quarter = 1,
   Half = 2,
   ThreeQuarters = 3,
   Full = 4
}
