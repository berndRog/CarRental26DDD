using CarRentalApi._4_BuildingBlocks._3_Domain.Entities;
namespace CarRentalApi._2_Modules.Cars._3_Domain.Aggregates;

public abstract class Vehicle : Entity<Guid> {
   // Guid Id is inherited from Entity<T>
   public string Manufacturer { get; private set; } = string.Empty;
   public string Model { get; private set; } = string.Empty;
   public string LicensePlate { get; private set; } = string.Empty;
   public DateTimeOffset CreatedAt { get; private set; }
   public DateTimeOffset? RetiredAt { get; protected set; }

   // EF Core ctor
   protected Vehicle() {
   }

   // Domain ctor (used by derived types)
   protected Vehicle(
      Guid id,
      string manufacturer,
      string model,
      string licensePlate,
      DateTimeOffset createdAt
   ) {
      Id = id;
      Manufacturer = manufacturer;
      Model = model;
      LicensePlate = licensePlate;
      CreatedAt = createdAt;
   }

   // Domain method to retire the vehicle
   protected void RetireAt(DateTimeOffset retiredAt) {
      // idempotent: if already retired, keep original date
      if (RetiredAt is not null) return;
      RetiredAt = retiredAt;
   }
}