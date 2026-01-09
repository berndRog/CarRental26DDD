namespace CarRentalApi.Modules.Cars.Domain.Enums;

public enum CarStatus {
   Available = 1,     // is available for rental
   Rented = 2,        // is actual rented
   Maintenance = 3,   // is under maintenance
   Retired = 4        // is retired from rental
}
