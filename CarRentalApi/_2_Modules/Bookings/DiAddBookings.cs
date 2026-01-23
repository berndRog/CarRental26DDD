using CarRentalApi._2_Modules.Bookings._2_Application.UseCases.Rental;
using CarRentalApi._2_Modules.Bookings._2_Application.UseCases.Reservation;
using CarRentalApi._2_Modules.Bookings._3_Domain.Policies;
using CarRentalApi._2_Modules.Bookings._4_Infrastructure.Policies;
using CarRentalApi._2_Modules.Bookings._4_Infrastructure.ReadModel;
using CarRentalApi._2_Modules.Bookings._4_Infrastructure.Repositories;
using CarRentalApi.Modules.Bookings;
using CarRentalApi.Modules.Bookings.Application.ReadModel;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Rentals.Application.UseCases;
namespace CarRentalApi._2_Modules.Bookings;

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
      services.AddScoped<IReservationConflictPolicy, ReservationConflictPolicyEf>();
      
      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<IReservationRepository, ReservationRepositoryEf>();
      services.AddScoped<IRentalRepository, RentalRepositoryEf>();

      return services;
   }
}