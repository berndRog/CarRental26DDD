using CarRentalApi.BuildingBlocks;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Errors;
using CarRentalApi.Modules.Bookings.Application.ReadModel.Mapping;
using CarRentalApi.Modules.Rentals.Application.ReadModel;
using CarRentalApi.Modules.Rentals.Application.ReadModel.Dto;
using Microsoft.EntityFrameworkCore;

namespace CarRentalApi.Modules.Bookings.Infrastructure.ReadModel;

public sealed class RentalReadModelEf(
   CarRentalDbContext _dbContext
) : IRentalReadModel {

   public async Task<Result<RentalDetailsDto>> FindByIdAsync(
      Guid rentalId,
      CancellationToken ct
   ) {
      if (rentalId == Guid.Empty)
         return Result<RentalDetailsDto>.Failure(RentalReadErrors.InvalidId);

      var rental = await _dbContext.Rentals
         .AsNoTracking()
         .FirstOrDefaultAsync(r => r.Id == rentalId, ct);

      return rental is null
         ? Result<RentalDetailsDto>.Failure(RentalReadErrors.NotFound)
         : Result<RentalDetailsDto>.Success(rental.ToRentalDetailsDto());
   }

   public async Task<Result<Guid?>> FindRentalIdByReservationIdAsync(
      Guid reservationId,
      CancellationToken ct
   ) {
      if (reservationId == Guid.Empty)
         return Result<Guid?>.Failure(RentalReadErrors.InvalidReservationId);

      var rental = await _dbContext.Rentals
         .AsNoTracking()
         .FirstOrDefaultAsync(r => r.ReservationId == reservationId, ct) ;
      
      return Result<Guid?>.Success(rental?.Id ?? null);
   }
}
