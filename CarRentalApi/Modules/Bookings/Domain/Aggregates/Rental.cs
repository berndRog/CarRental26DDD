using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Modules.Rentals.Domain.Errors;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Domain.Enums;
namespace CarRentalApi.Modules.Rentals.Domain.Aggregates;

/// <summary>
/// Rental lifecycle created at pick-up and closed at return.
/// CarId becomes known at pick-up (not in Reservation).
/// </summary>
public sealed class Rental: Entity<Guid> {
   
   // Guid ReservationId is inherited from Entity<T>
   
#if OOP_MODE   
   // With Navigation properties (object graph)
   
   // Car <-> Rental = 1 : 0..n
   public Guid CarId { get; private set; }
   public Car Car { get; private set; } = null!;
   
   // Reservation <-> Rental = 1 : 0..1
   public Guid ReservationId { get; private set; }
   public Reservation Reservation { get; private set; } = null!;
   
   // Customer <-> Rental = 1 : 0..n
   public Guid CustomerId { get; private set; }
   public Customer Customer { get; private set; } = null!;

#elif DDD_MODE
   // Without Navigation properties (foreign Keys only)
   // Use repositories to fetch related Car, Reservation, Customer
   
   // Car <-> Rental = 1 : 0..n as foreign key
   public Guid CarId { get; private set; }
   
   // Reservation <-> Rental = 1 : 0..1 as foreign key
   public Guid ReservationId { get; private set; }
   
   // Customer <-> Rental = 1 : 0..n as foreign key
   public Guid CustomerId { get; private set; }
#else
   #error "Define either OOP_MODE or DDD_MODE in .csproj"
#endif
   
   // Lifecycle
   public RentalStatus Status { get; private set; }

   // Pick-up data
   public DateTimeOffset PickupAt { get; private set; }
   public int FuelLevelOut { get; private set; } // 0..100
   public int KmOut { get; private set; } // >= 0

   // Return data
   public DateTimeOffset? ReturnAt { get; private set; }
   public int? FuelLevelIn { get; private set; } // 0..100
   public int? KmIn { get; private set; } // >= KmOut

   
   // EF Core
   private Rental() { }

   // Domain ctor
   private Rental(
      Guid id,
      Guid reservationId,
      Guid customerId,
      Guid carId,
      DateTimeOffset pickupAt,
      int fuelLevelOut,
      int kmOut
   ) {
      Id = id;
      ReservationId = reservationId;
      CustomerId = customerId;
      CarId = carId;

      Status = RentalStatus.Active;

      PickupAt = pickupAt;
      FuelLevelOut = fuelLevelOut;
      KmOut = kmOut;
   }

   //----------- Factory create at Pick-Up (Result-based) ----------
   public static Result<Rental> CreateAtPickup(
      Guid reservationId,
      Guid customerId,
      Guid carId,
      DateTimeOffset pickupAt,
      int fuelLevelOut,
      int kmOut,
      string? id = null
   ) {
      var idResult = EntityId.Resolve(id, RentalErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Rental>.Failure(idResult.Error);

      if (reservationId == Guid.Empty)
         return Result<Rental>.Failure(RentalErrors.InvalidReservation);

      if (customerId == Guid.Empty)
         return Result<Rental>.Failure(RentalErrors.InvalidCustomer);

      if (carId == Guid.Empty)
         return Result<Rental>.Failure(RentalErrors.InvalidCar);

      if (!IsValidFuel(fuelLevelOut))
         return Result<Rental>.Failure(RentalErrors.InvalidFuelLevel);

      if (kmOut < 0)
         return Result<Rental>.Failure(RentalErrors.InvalidKm);

      return Result<Rental>.Success(
         new Rental(
            id: idResult.Value,
            reservationId: reservationId,
            customerId: customerId,
            carId: carId,
            pickupAt: pickupAt,
            fuelLevelOut: fuelLevelOut,
            kmOut: kmOut
         )
      );
   }
   
   // ---------- Domain behavior -------------
   // Domain behavior: Return car
   public Result ReturnCar(
      DateTimeOffset returnAt,
      int fuelLevelIn,
      int kmIn
   ) {
      if (Status != RentalStatus.Active)
         return Result.Failure(RentalErrors.InvalidStatusTransition);

      if (returnAt < PickupAt)
         return Result.Failure(RentalErrors.InvalidTimestamp);

      if (!IsValidFuel(fuelLevelIn))
         return Result.Failure(RentalErrors.InvalidFuelLevel);

      if (kmIn < KmOut)
         return Result.Failure(RentalErrors.InvalidKm);

      ReturnAt = returnAt;
      FuelLevelIn = fuelLevelIn;
      KmIn = kmIn;

      Status = RentalStatus.Returned;
      return Result.Success();
   }

   // Convenience for policies
   public bool IsReturned() => Status == RentalStatus.Returned;

   public bool NeedsRefuelFee() {
      if (Status != RentalStatus.Returned) return false;
      return FuelLevelIn!.Value < FuelLevelOut;
   }

   private static bool IsValidFuel(int level) => level is >= 0 and <= 100;
}