using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Application.UseCases;
namespace CarRentalApi.Modules.Cars;

public static class DiAddCarsExtensions {
   
   public static IServiceCollection AddCars(
      this IServiceCollection services
   ) {
      
      services.AddScoped<CarUcCreate>();
      services.AddScoped<CarUcSendToMaintenance>();
      services.AddScoped<CarUcReturnFromMaintenance>();
      services.AddScoped<CarUcRetire>();
      services.AddScoped<ICarUseCases, CarUseCases>();

      return services;
   }
}