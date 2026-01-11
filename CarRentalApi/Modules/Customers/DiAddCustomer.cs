using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Application.UseCases;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using CarRentalApi.Modules.Customers.Application.Contracts;
using CarRentalApi.Modules.Customers.Application.ReadModel;
using CarRentalApi.Modules.Customers.Application.Services;
using CarRentalApi.Modules.Customers.Application.UseCases.Block;
namespace CarRentalApi.Modules.Cars;

public static class DiAddCustomersExtensions {
   
   public static IServiceCollection AddCustomers(
      this IServiceCollection services
   ) {
      
      // ReadModels
      //services.AddScoped<ICustomerReadModel, CustomerReadService>();
      
      // Services 
      services.AddScoped<ICustomerReadApi, CustomerReadService>();
      
      
      // Use Cases
      services.AddScoped<CustomerUcCreate>();
      services.AddScoped<CustomerUcBlock>();
      services.AddScoped<ICustomerUseCases, CustomerUseCases>();

      // Repositories
      services.AddScoped<ICustomerRepository, CustomerRepository>();
      
      return services;
   }
}