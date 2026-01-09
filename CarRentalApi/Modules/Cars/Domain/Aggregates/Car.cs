using System.Text.RegularExpressions;
using CarRentalApi.BuildingBlocks;
using CarRentalApi.BuildingBlocks.Domain.Entities;
using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Errors;
using CarRentalApi.Modules.Cars.Domain.Enums;
using CarRentalApi.Modules.Cars.Domain.Errors;
using CarRentalApi.Modules.Cars.Domain.Policies;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Modules.Reservations.Domain.ValueObjects;
namespace CarRentalApi.Modules.Cars.Domain.Aggregates;

public sealed class Car: Entity<Guid> {
   
   // Guid Id is inherited from Entity<T>
   
#if OOP_MODE
   // With Navigation properties (object graph)
   // Car <-> Rental = 1 : 0..n
   private readonly List<Rental> _rentals = new();
   public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

#elif DDD_MODE
   // Without Navigation properties
   // Use repositories to fetch related Cars

#else
   #error "Define either OOP_MODE or DDD_MODE in .csproj"
#endif
   
   public string Manufacturer { get; private set; } = string.Empty;
   public string Model { get; private set; } = string.Empty;
   public string LicensePlate { get; private set; } = string.Empty;

   // CarCategory is used for booking and capacity calculation.
   public CarCategory Category { get; private set; }
   public CarStatus Status { get; private set; }

   // EF Core ctor
   private Car() { }

   // Domain ctor
   private Car(
      Guid id,
      CarCategory category,
      string manufacturer,
      string model,
      string licensePlate
   ) {
      Id = id;
      Category = category;
      Manufacturer = manufacturer;
      Model = model;
      LicensePlate = licensePlate;
      Status = CarStatus.Available;
   }

   // ---------- Factory (Result-based) ----------
   public static Result<Car> Create(
      CarCategory category,
      string manufacturer,
      string model,
      string licensePlate,
      string? id = null
   ) {
      // Normalize input early
      manufacturer = manufacturer?.Trim() ?? string.Empty;
      model = model?.Trim() ?? string.Empty;
      licensePlate = licensePlate?.Trim() ?? string.Empty;
      
      if (!Enum.IsDefined(typeof(CarCategory), category))
         return Result<Car>.Failure(CarErrors.CategoryIsRequired);
      
      if (string.IsNullOrWhiteSpace(manufacturer))
         return Result<Car>.Failure(CarErrors.ManufacturerIsRequired);
      
      if (string.IsNullOrWhiteSpace(model))
         return Result<Car>.Failure(CarErrors.ModelIsRequired);
      
      if (string.IsNullOrWhiteSpace(licensePlate))
         return Result<Car>.Failure(CarErrors.LicensePlateIsRequired);
      
      // Only uppercase letters, digits and hyphens allowed
       if (!Regex.IsMatch(
             licensePlate,
             @"^[A-Z0-9\-]+$"))
         return Result<Car>.Failure(CarErrors.InvalidLicensePlateFormat);
      
      var idResult = EntityId.Resolve(id, CarErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Car>.Failure(idResult.Error);
      var carId = idResult.Value;
      
      return Result<Car>.Success(
         new Car(carId, category, manufacturer, model, licensePlate)
      );
   }

   
   /// <summary>
   /// Business rule: a car is available if there is no overlap with
   /// (1) active rentals and (2) confirmed reservations for the given period.
   /// </summary>
   public async Task<bool> IsAvailableAsync(
      RentalPeriod period,
      ICarAvailabilityReadModel availability,
      CancellationToken ct
   ) {
      if (Status != CarStatus.Available) return false;

      var hasOverlap = await availability.HasOverlapAsync(Id, period, ct);
      return !hasOverlap;
   }
   
   // ---------- CarStatus machine (centralized rules) --------
   private Result Transition(
      CarStatus from,
      CarStatus to, 
      DomainErrors error
   ) {
      // Removed/Retired cars cannot change status anymore.
      if (Status == CarStatus.Retired)
         return Result.Failure(CarErrors.InvalidStatusTransition);

      if (Status != from)
         return Result.Failure(error);

      Status = to;
      return Result.Success();
   }
   
   // ---------- Domain behavior ----------
   // Available -> Rented
   public Result MarkAsRented()
      => Transition(
         from: CarStatus.Available,
         to: CarStatus.Rented,
         error: CarErrors.CarNotAvailable
      );

   // Rented -> Available
   public Result MarkAsAvailable()
      => Transition(
         from: CarStatus.Rented,
         to: CarStatus.Available,
         error: CarErrors.InvalidStatusTransition
      );

   // Available -> Maintenance  (User Story 1.2)
   public Result SendToMaintenance()
      => Transition(
         from: CarStatus.Available,
         to: CarStatus.Maintenance,
         error: CarErrors.InvalidStatusTransition
      );

   // Maintenance -> Available (User Story 1.3)
   public Result ReturnFromMaintenance()
      => Transition(
         from: CarStatus.Maintenance,
         to: CarStatus.Available,
         error: CarErrors.InvalidStatusTransition
      );

   //---------- Retire car (User Story 1.4) ----------
   public Result Retire() {
      // strong invariant: once removed, lifecycle ends (idempotent)
      if (Status == CarStatus.Retired)
         return Result.Success();

      Status = CarStatus.Retired;
      return Result.Success();
   }

}