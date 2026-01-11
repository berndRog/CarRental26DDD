using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.BuildingBlocks.Utils;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.Aggregates;
using CarRentalApi.Modules.Customers.Domain.ValueObjects;
using CarRentalApi.Modules.Rentals.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Domain.Aggregates;
using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
namespace CarRentalApiTest;

public sealed class TestSeed {

   public DateTimeOffset FixedNow => DateTimeOffset.Parse("2025-01-01T00:00:00Z");

   // ---------- Test data for addresses ----------
   public Address Address1 { get; private set; } = null!;
   public Address Address2 { get; private set; } = null!;
   public Address Address3 { get; private set;} = null!; 
   
   //---------- Test data for customers ----------
   public string Customer1Id = "00000000-0001-0000-0000-000000000000";
   public string Customer2Id = "00000000-0002-0000-0000-000000000000";
   public string Customer3Id = "00000000-0003-0000-0000-000000000000";
   public string Customer4Id = "00000000-0004-0000-0000-000000000000";
   public string Customer5Id = "00000000-0005-0000-0000-000000000000";
   
   public Customer Customer1 { get; private set; } = null!;
   public Customer Customer2 { get; private set; } = null!;
   public Customer Customer3 { get; private set; } = null!;
   public Customer Customer4 { get; private set; } = null!;
   public Customer Customer5 { get; private set; } = null!;
   
   public IReadOnlyList<Customer> Customers => [
      Customer1, Customer2, Customer3, Customer4, Customer5
   ];

   //---------- Test data for cars ----------
   public string Car1Id = "00000000-0000-0001-0000-000000000000";
   public string Car2Id = "00000000-0000-0002-0000-000000000000";
   public string Car3Id = "00000000-0000-0003-0000-000000000000";
   public string Car4Id = "00000000-0000-0004-0000-000000000000";
   public string Car5Id = "00000000-0000-0005-0000-000000000000";
   public string Car6Id = "00000000-0000-0006-0000-000000000000";
   public string Car7Id = "00000000-0000-0007-0000-000000000000";
   public string Car8Id = "00000000-0000-0008-0000-000000000000";
   public string Car9Id = "00000000-0000-0009-0000-000000000000";
   public string Car10Id = "00000000-0000-0010-0000-000000000000";
   public string Car11Id = "00000000-0000-0011-0000-000000000000";
   public string Car12Id = "00000000-0000-0012-0000-000000000000";
   public string Car13Id = "00000000-0000-0013-0000-000000000000";
   public string Car14Id = "00000000-0000-0014-0000-000000000000";
   public string Car15Id = "00000000-0000-0015-0000-000000000000";
   public string Car16Id = "00000000-0000-0016-0000-000000000000";
   public string Car17Id = "00000000-0000-0017-0000-000000000000";
   public string Car18Id = "00000000-0000-0018-0000-000000000000";
   public string Car19Id = "00000000-0000-0019-0000-000000000000";
   public string Car20Id = "00000000-0000-0020-0000-000000000000";

   public Car Car1 { get; private set; } = null!;
   public Car Car2 { get; private set; } = null!;
   public Car Car3 { get; private set; } = null!;
   public Car Car4 { get; private set; } = null!;
   public Car Car5 { get; private set; } = null!;
   public Car Car6 { get; private set; } = null!;
   public Car Car7 { get; private set; } = null!;
   public Car Car8 { get; private set; } = null!;
   public Car Car9 { get; private set; } = null!;
   public Car Car10 { get; private set; } = null!;
   public Car Car11 { get; private set; } = null!;
   public Car Car12 { get; private set; } = null!;
   public Car Car13 { get; private set; } = null!;
   public Car Car14 { get; private set; } = null!;
   public Car Car15 { get; private set; } = null!;
   public Car Car16 { get; private set; } = null!;
   public Car Car17 { get; private set; } = null!;
   public Car Car18 { get; private set; } = null!;
   public Car Car19 { get; private set; } = null!;
   public Car Car20 { get; private set; } = null!;

   public IReadOnlyList<Car> Cars => [
      Car1, Car2, Car3, Car4, Car5,
      Car6, Car7, Car8, Car9, Car10,
      Car11, Car12, Car13, Car14, Car15,
      Car16, Car17, Car18, Car19, Car20
   ];

   // ---------- Test data for reservations ----------
   public string Reservation1Id = "00100000-0000-0000-0000-000000000000";
   public string Reservation2Id = "00200000-0000-0000-0000-000000000000";
   public string Reservation3Id = "00300000-0000-0000-0000-000000000000";
   public string Reservation4Id = "00400000-0000-0000-0000-000000000000";
   public string Reservation5Id = "00500000-0000-0000-0000-000000000000";
   public string Reservation6Id = "00600000-0000-0000-0000-000000000000";
   public string Reservation7Id = "00700000-0000-0000-0000-000000000000";
   public string Reservation8Id = "00800000-0000-0000-0000-000000000000";
   public string Reservation9Id = "00900000-0000-0000-0000-000000000000";
   public string Reservation10Id = "01000000-0000-0000-0000-000000000000";
   
   public Reservation Reservation1 { get; private set; } = null!;
   public Reservation Reservation2 { get; private set; } = null!;
   public Reservation Reservation3 { get; private set; } = null!;
   public Reservation Reservation4 { get; private set; } = null!;
   public Reservation Reservation5 { get; private set; } = null!;
   public Reservation Reservation6 { get; private set; } = null!;
   public Reservation Reservation7 { get; private set; } = null!;
   public Reservation Reservation8 { get; private set; } = null!;
   public Reservation Reservation9 { get; private set; } = null!;
   public Reservation Reservation10 { get; private set; } = null!;

   public IReadOnlyList<Reservation> Reservations => [
      Reservation1, Reservation2, Reservation3, Reservation4,
      Reservation5, Reservation6, Reservation7, Reservation8,
      Reservation9, Reservation10
   ];
   
   public IReadOnlyList<Reservation> ReservationsOverlappingConfirmed => [
      Reservation1, Reservation2, Reservation3, Reservation4,
      Reservation5, Reservation6, Reservation7, Reservation8
   ];

   // ---------- Test data for rentals ----------
   public string Rental1Id = "10000000-0000-0000-0000-000000000000";
   public string Rental2Id = "20000000-0000-0000-0000-000000000000";
   public string Rental3Id = "30000000-0000-0000-0000-000000000000";

   public Rental Rental1 { get; private set; } = null!;
   public Rental Rental2 { get; private set; } = null!;
   public Rental Rental3 { get; private set; } = null!;

   public IReadOnlyList<Rental> Rentals => [
      Rental1, Rental2, Rental3
   ];

   
   //---------- Common test periods ----------
   // Periods for overlap/non-overlap scenarios
   public RentalPeriod Period1 => RentalPeriod.Create(
      DateTimeOffset.Parse("2030-05-01T10:00:00+00:00"),
      DateTimeOffset.Parse("2030-05-05T10:00:00+00:00")
   ).GetValueOrThrow();

   public RentalPeriod Period2 => RentalPeriod.Create(
      DateTimeOffset.Parse("2030-05-11T10:00:00+00:00"),
      DateTimeOffset.Parse("2030-05-15T10:00:00+00:00")
   ).GetValueOrThrow();

   // Overlapping with Period1 (still overlaps because it starts inside)
   public RentalPeriod PeriodOverlap1 => RentalPeriod.Create(
      DateTimeOffset.Parse("2030-05-03T10:00:00+00:00"),
      DateTimeOffset.Parse("2030-05-07T10:00:00+00:00")
   ).GetValueOrThrow();

   // Fully outside (non-overlapping with Period1)
   public RentalPeriod PeriodOkNonOverlapping => RentalPeriod.Create(
      DateTimeOffset.Parse("2030-06-01T10:00:00+00:00"),
      DateTimeOffset.Parse("2030-06-05T10:00:00+00:00")
   ).GetValueOrThrow();

   // Draft should be expired when now >= createdAt
   public DateTimeOffset DraftCreatedAtOld =>
      DateTimeOffset.Parse("2025-12-01T10:00:00+00:00");

   public TestSeed() {
      //---------- Addresses ----------
      Address1 = Address.Create("Hauptstr. 23", "29556", "Suderburg").GetValueOrThrow();
      Address2 = Address.Create("Bahnhofstr.10", "10115", "Berlin").GetValueOrThrow();
      Address3 = Address.Create("Schillerstr. 1", "30123", "Hannover").GetValueOrThrow();
      
      //---------- Customers ----------
      Customer1 = CreateCustomer(Customer1Id, "Erika", "Mustermann","e.mustermann@t-line.de",FixedNow, Address1);
      Customer2 = CreateCustomer(Customer2Id, "Max", "Mustermann","m.mustermann@gmail.com",FixedNow);
      Customer3 = CreateCustomer(Customer3Id, "Arne", "Arndt", "a.arndt@icloud.com", FixedNow, Address2);
      Customer4 = CreateCustomer(Customer4Id, "Benno", "Bauer", "b.bauer@t-online.de", FixedNow);
      Customer5 = CreateCustomer(Customer5Id, "Chrisitine","Conrad", "c.conrad@gmx.de", FixedNow, Address3);
      
      //---------- Cars ----------
      Car1 = CreateCar(Car1Id, CarCategory.Economy, "VW", "Polo", "ECO-001");
      Car2 = CreateCar(Car2Id, CarCategory.Economy, "VW", "Polo", "ECO-002");
      Car3 = CreateCar(Car3Id, CarCategory.Economy, "VW", "Polo", "ECO-003");
      Car4 = CreateCar(Car4Id, CarCategory.Economy, "VW", "Polo", "ECO-004");
      Car5 = CreateCar(Car5Id, CarCategory.Economy, "VW", "Polo", "ECO-005");

      Car6 = CreateCar(Car6Id, CarCategory.Compact, "VW", "Golf", "COM-001");
      Car7 = CreateCar(Car7Id, CarCategory.Compact, "VW", "Golf", "COM-002");
      Car8 = CreateCar(Car8Id, CarCategory.Compact, "VW", "Golf", "COM-003");
      Car9 = CreateCar(Car9Id, CarCategory.Compact, "VW", "Golf", "COM-004");
      Car10 = CreateCar(Car10Id, CarCategory.Compact, "VW", "Golf", "COM-005");

      Car11 = CreateCar(Car11Id, CarCategory.Midsize, "BMW", "3 Series", "MID-001");
      Car12 = CreateCar(Car12Id, CarCategory.Midsize, "BMW", "3 Series", "MID-002");
      Car13 = CreateCar(Car13Id, CarCategory.Midsize, "BMW", "3 Series", "MID-003");
      Car14 = CreateCar(Car14Id, CarCategory.Midsize, "BMW", "3 Series", "MID-004");
      Car15 = CreateCar(Car15Id, CarCategory.Midsize, "BMW", "3 Series", "MID-005");

      Car16 = CreateCar(Car16Id, CarCategory.Suv, "Audi", "Q5", "SUV-001");
      Car17 = CreateCar(Car17Id, CarCategory.Suv, "Audi", "Q5", "SUV-002");
      Car18 = CreateCar(Car18Id, CarCategory.Suv, "Audi", "Q5", "SUV-003");
      Car19 = CreateCar(Car19Id, CarCategory.Suv, "Audi", "Q5", "SUV-004");
      Car20 = CreateCar(Car20Id, CarCategory.Suv, "Audi", "Q5", "SUV-005");
      
      //---------- Reservations ----------
      // Reservations (raw):
      //  1–8  intended to be confirmed and overlapping with Period1
      //    9  intended to be confirmed and non-overlapping
      //   10  intended to stay draft and expire

      var createdAtConfirmed = DateTimeOffset.Parse("2025-12-25T10:00:00+00:00");

      Reservation1 = CreateReservation(Reservation1Id, Customer1Id, CarCategory.Compact, Period1, 
         createdAtConfirmed);
      Reservation2 = CreateReservation(Reservation2Id, Customer1Id, CarCategory.Compact, PeriodOverlap1,
         createdAtConfirmed);
      Reservation3 = CreateReservation(Reservation3Id, Customer1Id, CarCategory.Compact, Period1, 
         createdAtConfirmed);
      Reservation4 = CreateReservation(Reservation4Id, Customer2Id, CarCategory.Compact, PeriodOverlap1,
         createdAtConfirmed);
      Reservation5 = CreateReservation(Reservation5Id, Customer2Id, CarCategory.Compact, Period1, 
         createdAtConfirmed);
      Reservation6 = CreateReservation(Reservation6Id, Customer3Id, CarCategory.Compact, PeriodOverlap1,
         createdAtConfirmed);
      Reservation7 = CreateReservation(Reservation7Id, Customer3Id, CarCategory.Compact, Period1, 
         createdAtConfirmed);
      Reservation8 = CreateReservation(Reservation8Id, Customer4Id, CarCategory.Compact, PeriodOverlap1,
         createdAtConfirmed);
      // NOT overlapping with Period1
      Reservation9 = CreateReservation(
         id: Reservation9Id,
         customerId: Customer1Id,
         carCategory: CarCategory.Compact,
         period: PeriodOkNonOverlapping,
         createdAt: createdAtConfirmed
      );

      // Draft to expire
      Reservation10 = CreateReservation(
         id: Reservation10Id,
         customerId: Customer1Id,
         carCategory: CarCategory.Compact,
         period: Period1,
         createdAt: DraftCreatedAtOld
      );
      
      // ---------- Rentals (created at pick-up, raw / active) ----------
      // Note:
      // - CarId becomes known here (not in Reservation)
      // - Rentals start in CarStatus = Active
      // - No returns performed in TestSeed

      var pickupAt = DateTimeOffset.Parse("2030-05-01T10:05:00+00:00");

      Rental1 = CreateRental(
         id: Rental1Id,
         reservation: Reservation1,
         customer: Customer1,
         car: Car6,              // Compact
         pickupAt: pickupAt,
         fuelOut: 80,
         kmOut: 10_000
      );

      Rental2 = CreateRental(
         id: Rental2Id,
         reservation: Reservation2,
         customer: Customer1,
         car: Car7,
         pickupAt: pickupAt.AddHours(1),
         fuelOut: 70,
         kmOut: 20_000
      );

      Rental3 = CreateRental(
         id: Rental3Id,
         reservation: Reservation3,
         customer: Customer2,
         car: Car8,
         pickupAt: pickupAt.AddHours(2),
         fuelOut: 90,
         kmOut: 30_000
      );

   }

   // ---------- Helper ----------
   private static Customer CreateCustomer(
      string id,
      string firstName,
      string lastName,
      string email,
      DateTimeOffset createdAt
   ) {
      var result = Customer.Create(
         createdAt: createdAt,
         id: id
      );

      Assert.True(result.IsSuccess);
      return result.Value!;
   }

   private static Customer CreateCustomer(
      string id,
      string firstName,
      string lastName,
      string email,
      DateTimeOffset createdAt,
      Address? address
   ) {
      var result = Customer.Create(
         createdAt: createdAt,
         id: id
      );

      Assert.True(result.IsSuccess);
      return result.Value!;
   }
   
   private static Car CreateCar(
      string id,
      CarCategory category,
      string manufacturer,
      string model,
      string licensePlate
   ) {
      var result = Car.Create(
         category: category,
         manufacturer: manufacturer,
         model: model,
         licensePlate: licensePlate,
         id: id
      );

      // Test seed must always be valid
      Assert.True(result.IsSuccess);
      return result.Value!;
   }
   
   private static Reservation CreateReservation(
      string id,
      string customerId,
      CarCategory carCategory,
      RentalPeriod period,
      DateTimeOffset createdAt
   ) {
      var result = Reservation.Create(
         customerId: customerId.ToGuid(),
         carCategory: carCategory,
         start: period.Start,
         end: period.End,
         createdAt: createdAt,
         id: id
      );

      // Test seed must always be valid
      Assert.True(result.IsSuccess);
      return result.Value!;
   }
   
   private static Rental CreateRental(
      string id,
      Reservation reservation,
      Customer customer,
      Car car,
      DateTimeOffset pickupAt,
      int fuelOut,
      int kmOut
   ) {
      var result = Rental.CreateAtPickup(
         reservationId: reservation.Id,
         customerId: customer.Id,
         carId: car.Id,
         pickupAt: pickupAt,
         fuelLevelOut: fuelOut,
         kmOut: kmOut,
         id: id
      );

      // Test seed must always be valid
      Assert.True(result.IsSuccess);
      return result.Value!;
   }

}