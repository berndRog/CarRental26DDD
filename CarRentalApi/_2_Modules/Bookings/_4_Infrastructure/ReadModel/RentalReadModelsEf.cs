using CarRentalApi._2_Modules.Bookings._2_Application.Dtos.ReadModels;
using CarRentalApi._2_Modules.Bookings._2_Application.Errors;
using CarRentalApi._2_Modules.Bookings._2_Application.Mappings;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi.Data.Database;
using CarRentalApi.Modules.Rentals.Application.ReadModel;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi._2_Modules.Bookings._4_Infrastructure.ReadModel;

public sealed class RentalReadModelEf(
   CarRentalDbContext _dbContext
) : IRentalReadModel {

   public async Task<Result<RentalDetailsDto>> FindByIdAsync(
      Guid rentalId,
      CancellationToken ct
   ) {
      if (rentalId == Guid.Empty)
         return Result<RentalDetailsDto>.Failure(RentalApplicationErrors.InvalidId);

      var rental = await _dbContext.Rentals
         .AsNoTracking()
         .FirstOrDefaultAsync(r => r.Id == rentalId, ct);

      return rental is null
         ? Result<RentalDetailsDto>.Failure(RentalApplicationErrors.NotFound)
         : Result<RentalDetailsDto>.Success(rental.ToRentalDetailsDto());
   }

   public async Task<Result<Guid?>> FindRentalIdByReservationIdAsync(
      Guid reservationId,
      CancellationToken ct
   ) {
      if (reservationId == Guid.Empty)
         return Result<Guid?>.Failure(RentalApplicationErrors.InvalidReservationId);

      var rental = await _dbContext.Rentals
         .AsNoTracking()
         .FirstOrDefaultAsync(r => r.ReservationId == reservationId, ct) ;
      
      return Result<Guid?>.Success(rental?.Id ?? null);
   }
}
