using CarRentalApi.BuildingBlocks.Enums;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
using CarRentalApi.Modules.Cars.Domain.Aggregates;
using CarRentalApi.Modules.Cars.Domain.Enums;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Modules.Cars.Infrastructure.ReadModel;

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