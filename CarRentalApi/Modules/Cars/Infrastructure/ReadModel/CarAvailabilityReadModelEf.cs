using Microsoft.EntityFrameworkCore;
using CarRentalApi.Data.Database; // ggf. anpassen
using CarRentalApi.Modules.Cars.Domain.Policies;
using CarRentalApi.Modules.Rentals.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.Enums;
using CarRentalApi.Modules.Bookings.Domain.ValueObjects;
using CarRentalApi.Modules.Cars.Application.ReadModel;

namespace CarRentalApi.Modules.Cars.Infrastructure.ReadModels;

/// <summary>
/// EF Core backed implementation of ICarAvailabilityReadModel.
///
/// Domain meaning:
/// - Reservations are made by CarCategory (no CarId)
/// - A concrete car is blocked only by ACTIVE rentals
/// - Rental has no own period -> time window is derived from the referenced Reservation
/// </summary>
public sealed class CarAvailabilityReadModelEf(
   CarRentalDbContext _db
) : ICarAvailabilityReadModel
{
   public async Task<bool> HasOverlapAsync(
      Guid carId,
      RentalPeriod period,
      CancellationToken ct
   )
   {
      var start = period.Start;
      var end   = period.End;

      // Half-open interval overlap:
      // [a,b) overlaps [c,d)  <=>  a < d && c < b
      return await (
         from rental in _db.Rentals.AsNoTracking()
         join res in _db.Reservations.AsNoTracking()
            on rental.ReservationId equals res.Id
         where rental.CarId == carId
         where rental.Status == RentalStatus.Active
         where res.Status == ReservationStatus.Confirmed
         where res.Period.Start < end && start < res.Period.End
         select rental.Id
      ).AnyAsync(ct);
   }
}