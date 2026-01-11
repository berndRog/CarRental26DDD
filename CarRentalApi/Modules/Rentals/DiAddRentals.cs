using CarRentalApi.Domain.UseCases.Rentals;
using CarRentalApi.Infrastructure.Persistence.Repositories;
using CarRentalApi.Modules.Bookings.Application.UseCases;
using CarRentalApi.Modules.Bookings.Domain;
namespace CarRentalApi.Modules.Rentals;

public static class DiAddRentalsExtensions {
   
   public static IServiceCollection AddRentals(
      this IServiceCollection services
   ) {
      
      services.AddScoped<RentalUcPickup>();
      services.AddScoped<RentalUcReturn>();
      services.AddScoped<IRentalUseCases, RentalUseCases>();

      
      // Repositories
      services.AddScoped<IRentalRepository, RentalRepository>();
      
      return services;
   }
}