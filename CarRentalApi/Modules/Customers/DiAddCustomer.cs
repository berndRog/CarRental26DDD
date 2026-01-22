using CarRentalApi.BuildingBlocks.Ports.Outbound;
using CarRentalApi.Domain;
using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Application.UseCases;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using CarRentalApi.Modules.Customers.Application.Contracts;
using CarRentalApi.Modules.Customers.Application.ReadModel;
using CarRentalApi.Modules.Customers.Application.Services;
using CarRentalApi.Modules.Customers.Application.UseCases.Block;
using CarRentalApi.Modules.Customers.Infrastructure.ReadModel;
namespace CarRentalApi.Modules.Customers;

public static class DiAddCustomersExtensions {
   
   public static IServiceCollection AddCustomers(
      this IServiceCollection services
   ) {
      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // Contracts      
      services.AddScoped<ICustomerReadContract, CustomerReadContractServiceEf>();

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
      // Gateways security
      services.AddScoped<IIdentityGateway, CustomerIdentityGatewayEf>();
      
      
      // Repositories
      services.AddScoped<ICustomerRepository, CustomerRepositoryEf>();
      
      return services;
   }

}