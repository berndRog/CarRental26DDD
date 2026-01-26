using CarRentalApi._2_Modules.Customers._1_Ports.Inbound;
using CarRentalApi._2_Modules.Customers._1_Ports.Outbound;
using CarRentalApi._2_Modules.Customers._2_Application.UseCases;
using CarRentalApi._2_Modules.Customers._4_Infrastructure.Adapters;
using CarRentalApi._2_Modules.Customers._4_Infrastructure.ReadModels;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using CarRentalApi.Modules.Customers._1_Ports.Inbound;
namespace CarRentalApi._2_Modules.Customers;

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
      services.AddScoped<CustomerUcProvisioned>();
      services.AddScoped<CustomerUcProfile>();
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