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
   
   // Guid Id is inherited from Entity<T>
   
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
   public RentalFuelLevel FuelOut { get; private set; } 
   public int KmOut { get; private set; } // >= 0

   // Return data
   public DateTimeOffset? ReturnAt { get; private set; }
   public RentalFuelLevel? FuelIn { get; private set; } 
   public int? KmIn { get; private set; } // >= KmOut
   public bool IsReturned() => Status == RentalStatus.Returned;

   
   // EF Core
   private Rental() { }

   // Domain ctor
   private Rental(
      Guid id,
      Guid reservationId,
      Guid customerId,
      Guid carId,
      DateTimeOffset pickupAt,
      RentalFuelLevel fuelOut,
      int kmOut
   ) {
      Id = id;
      ReservationId = reservationId;
      CustomerId = customerId;
      CarId = carId;

      Status = RentalStatus.Active;

      PickupAt = pickupAt;
      FuelOut = fuelOut;
      KmOut = kmOut;
   }

   //----------- Factory create at Pick-Up (Result-based) ----------
   public static Result<Rental> CreateAtPickup(
      Guid reservationId,
      Guid customerId,
      Guid carId,
      DateTimeOffset pickupAt,
      RentalFuelLevel fuelOut,
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
      
      if (!Enum.IsDefined(typeof(RentalFuelLevel), fuelOut))
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
            fuelOut: fuelOut,
            kmOut: kmOut
         )
      );
   }
   
   // ---------- Domain behavior -------------
   // Domain behavior: Return car
   public Result ReturnCar(
      DateTimeOffset returnAt,
      RentalFuelLevel fuelIn,
      int kmIn
   ) {
      if (Status != RentalStatus.Active)
         return Result.Failure(RentalErrors.InvalidStatusTransition);

      if (returnAt < PickupAt)
         return Result.Failure(RentalErrors.InvalidTimestamp);

      if (!Enum.IsDefined(typeof(RentalFuelLevel), fuelIn))
         return Result.Failure(RentalErrors.InvalidFuelLevel);
      
      if (kmIn < KmOut)
         return Result.Failure(RentalErrors.InvalidKm);

      ReturnAt = returnAt;
      FuelIn = fuelIn;
      KmIn = kmIn;

      Status = RentalStatus.Returned;
      return Result.Success();
   }
   
   // public bool NeedsRefuelFee() {
   //    if (Status != RentalStatus.Returned) return false;
   //    return FuelIn!.Value < FuelOut;
   // }

}