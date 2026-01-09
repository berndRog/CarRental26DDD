using CarRentalApi.Domain;
using CarRentalApi.Modules.Rentals;
using CarRentalApi.Modules.Reservations.Application;
using CarRentalApi.Modules.Reservations.Application.Contracts;
using CarRentalApi.Modules.Reservations.Application.Services;
using CarRentalApi.Modules.Reservations.Application.UseCases;
using CarRentalApi.Modules.Reservations.Domain;
using CarRentalApi.Modules.Reservations.Domain.Policies;
using CarRentalApi.Modules.Reservations.Infrastructure;
using CarRentalApi.Modules.Reservations.Infrastructure.Repositories;
namespace CarRentalApi.Modules.Reservations;

public static class DiAddReservations {
   
   public static IServiceCollection AddReservationExtensions(
      this IServiceCollection services
   ) {
      services.AddScoped<ReservationUcCreate>();
      services.AddScoped<ReservationUcChangePeriod>();
      services.AddScoped<ReservationUcConfirm>();      
      services.AddScoped<ReservationUcCancel>();
      services.AddScoped<ReservationUcExpire>();
      services.AddScoped<IReservationUseCases, ReservationUseCases>();
      
      services.AddScoped<IReservationConflictPolicy, ReservationConflictPolicy>();
      
      services.AddScoped<IReservationsReadApi, ReservationsReadService>();
      services.AddScoped<IReservationsWriteApi, ReservationsWriteService>();
      services.AddScoped<IReservationRepository, ReservationRepository>();

      return services;
   }
}