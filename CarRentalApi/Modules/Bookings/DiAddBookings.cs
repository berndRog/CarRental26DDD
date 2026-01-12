using CarRentalApi.Domain.UseCases.Rentals;
using CarRentalApi.Modules.Bookings.Application.ReadModel;
using CarRentalApi.Modules.Bookings.Application.UseCases;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Bookings.Domain.Policies;
using CarRentalApi.Modules.Bookings.Infrastructure.ReadModel;
using CarRentalApi.Modules.Bookings.Infrastructure.Repositories;
using CarRentalApi.Modules.Rentals;
using CarRentalApi.Modules.Rentals.Application.UseCases;
namespace CarRentalApi.Modules.Bookings;

public static class DiAddBookings {
   
   public static IServiceCollection AddBookings(
      this IServiceCollection services
   ) {
      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // ReadModels (Queries)     
      services.AddScoped<IReservationReadModel, ReservationReadModelEf>();

      // WriteModels = Use Cases
      services.AddScoped<ReservationUcCreate>();
      services.AddScoped<ReservationUcChangePeriod>();
      services.AddScoped<ReservationUcConfirm>();      
      services.AddScoped<ReservationUcCancel>();
      services.AddScoped<ReservationUcExpire>();
      services.AddScoped<IReservationUseCases, ReservationUseCases>();
      
      services.AddScoped<RentalUcPickup>();
      services.AddScoped<RentalUcReturn>();
      services.AddScoped<IRentalUseCases, RentalUseCases>();
      
      // Policies
      services.AddScoped<IReservationConflictPolicy, ReservationConflictPolicy>();
      
      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<IReservationRepository, ReservationRepositoryEf>();
      services.AddScoped<IRentalRepository, RentalRepositoryEf>();

      return services;
   }
}