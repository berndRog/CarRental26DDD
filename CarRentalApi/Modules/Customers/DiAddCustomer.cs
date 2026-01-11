using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Application.UseCases;
using CarRentalApi.Modules.Cars.Domain.Policies;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using CarRentalApi.Modules.Cars.Ports.Outbound;
using CarRentalApi.Modules.Customers.Application.Contracts;
using CarRentalApi.Modules.Customers.Application.ReadModel;
using CarRentalApi.Modules.Customers.Application.Services;
using CarRentalApi.Modules.Customers.Application.UseCases.Block;
using CarRentalApi.Modules.Customers.Infrastructure.ReadModel;
namespace CarRentalApi.Modules.Cars;

public static class DiAddCustomersExtensions {
   
   public static IServiceCollection AddCustomers(
      this IServiceCollection services
   ) {
      

      // =========================================================      
      // Contracts BC-to-BC
      // =========================================================
      services.AddScoped<ICustomerReadContract, CustomerReadContractServiceEf>();
      
      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // ReadModels (Queries)
      services.AddScoped<ICustomerReadModel, CustomerReadModelEf>();
      
      // WriteModels = Use Cases
      services.AddScoped<CustomerUcCreate>();
      services.AddScoped<CustomerUcBlock>();
      services.AddScoped<ICustomerUseCases, CustomerUseCases>();
      
      // Policies
      
      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<ICustomerRepository, CustomerRepositoryEf>();
      
      return services;
   }

}