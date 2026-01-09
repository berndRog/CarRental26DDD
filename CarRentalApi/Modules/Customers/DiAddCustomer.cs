using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Application.UseCases;
namespace CarRentalApi.Modules.Cars;

public static class DiAddCustomersExtensions {
   
   public static IServiceCollection AddCustomers(
      this IServiceCollection services
   ) {
      
      services.AddScoped<CustomerUcCreate>();
      services.AddScoped<ICustomerUseCases, CustomerUseCases>();

      return services;
   }
}