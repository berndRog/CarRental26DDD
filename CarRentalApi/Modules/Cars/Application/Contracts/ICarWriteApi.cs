using CarRentalApi.BuildingBlocks;

namespace CarRentalApi.Modules.Cars.Application.Contracts;

/// <summary>
/// Command facade for the Cars (Fleet Management) bounded context.
///
/// This API exposes write operations that change the lifecycle
/// or availability state of cars.
/// 
/// Characteristics:
/// - Write-only (commands)
/// - Modifies car state
/// - Enforces business rules and invariants
/// - Returns domain-level results (no DTOs)
/// </summary>
public interface ICarWriteApi {

   /// <summary>
   /// Sends a car to maintenance.
   ///
   /// Business meaning:
   /// - The car is temporarily removed from the active fleet
   /// - The car cannot be rented while in maintenance
   ///
   /// Typical use cases:
   /// - Damage repair
   /// - Scheduled service
   ///
   /// Expected rules:
   /// - Car must exist
   /// - Car must not already be in maintenance
   /// - Car must not be currently rented
   ///
   /// Returns:
   /// - Success if the car state was changed to "InMaintenance"
   /// - Failure if any business rule is violated
   /// </summary>
   Task<Result> SendToMaintenanceAsync(
      Guid carId,
      CancellationToken ct
   );

   /// <summary>
   /// Returns a car from maintenance back to the active fleet.
   ///
   /// Business meaning:
   /// - The car becomes available for rental again
   ///
   /// Typical use cases:
   /// - Maintenance or repair has been completed
   ///
   /// Expected rules:
   /// - Car must exist
   /// - Car must currently be in maintenance
   ///
   /// Returns:
   /// - Success if the car state was changed to "Available"
   /// - Failure if the car is not in maintenance or does not exist
   /// </summary>
   Task<Result> ReturnFromMaintenanceAsync(
      Guid carId,
      CancellationToken ct
   );

   /// <summary>
   /// Marks a car as rented.
   ///
   /// Business meaning:
   /// - The car is assigned to an active rental
   /// - The car is no longer available for new rentals
   ///
   /// Typical use cases:
   /// - Pick-up process when a rental starts
   ///
   /// Expected rules:
   /// - Car must exist
   /// - Car must be available
   /// - Car must not be in maintenance
   ///
   /// Returns:
   /// - Success if the car state was changed to "Rented"
   /// - Failure if the car is not available or does not exist
   /// </summary>
   Task<Result> MarkAsRentedAsync(
      Guid carId,
      CancellationToken ct
   );

   /// <summary>
   /// Marks a car as available again after a rental has been completed.
   ///
   /// Business meaning:
   /// - The car is no longer assigned to an active rental
   /// - The car can be rented again unless it is sent to maintenance
   ///
   /// Typical use cases:
   /// - Return process when a rental is closed
   ///
   /// Expected rules:
   /// - Car must exist
   /// - Car must currently be rented
   /// - Car must not be in maintenance
   ///
   /// Returns:
   /// - Success if the car state was changed to "Available"
   /// - Failure if the car is not rented or does not exist
   /// </summary>
   Task<Result> MarkAvailableAsync(
      Guid carId,
      CancellationToken ct
   );
}