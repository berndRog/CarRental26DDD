using CarRentalApi._2_Modules.Bookings._3_Domain.Enums;
using CarRentalApi._2_Modules.Bookings._3_Domain.ValueObjects;
using CarRentalApi._2_Modules.Cars._3_Domain.Aggregates;
using CarRentalApi._2_Modules.Cars._3_Domain.Enums;
using CarRentalApi._3_Infrastructure.Persistence.Database;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi._2_Modules.Cars._4_Infrastructure.ReadModel;

internal static class CarAvailabilityEfQueries {

   internal static IQueryable<Car> BuildAvailableCarsQuery(
      CarRentalDbContext db,
      CarCategory category,
      RentalPeriod period
   ) {
      var start = period.Start;
      var end = period.End;
      
      // LINQ query syntax
      var blockedCarIds = db.Rentals
         .AsNoTracking()
         .Where(rental => rental.Status == RentalStatus.Active)
         .Join(
            db.Reservations.AsNoTracking(),
            rental => rental.ReservationId,     // ON rental.ReservationId == reservation.Id
            reservation    => reservation.Id,
            (rental, reservation) => new { rental.CarId, Res = reservation }
         )
         .Where(x => x.Res.Status == ReservationStatus.Confirmed)  
         .Where(x => x.Res.Period.Start < end && start < x.Res.Period.End)
         .Select(x => x.CarId);

      // LINQ method syntax
      // var blockedCarIds = db.Rentals
      //    .AsNoTracking()
      //    .Where(rental => rental.Status == RentalStatus.Active)
      //    .Where(rental =>
      //       db.Reservations.AsNoTracking().Any(reservation =>
      //          reservation.Id == rental.ReservationId
      //          && reservation.Status == ReservationStatus.Confirmed
      //          && reservation.Period.Start < end
      //          && start < reservation.Period.End
      //       )
      //    )
      //    .Select(rental => rental.CarId);

      return db.Cars
         .AsNoTracking()
         .Where(c =>
            c.Category == category &&
            c.Status == CarStatus.Available &&
            c.RetiredAt == null
         )
         .Where(c => !blockedCarIds.Contains(c.Id));
   }
}