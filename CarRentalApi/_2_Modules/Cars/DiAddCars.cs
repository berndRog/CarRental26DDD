using CarRentalApi._2_Modules.Cars._1_Ports.Inbound;
using CarRentalApi._2_Modules.Cars._1_Ports.Outbound;
using CarRentalApi._2_Modules.Cars._2_Application.Pricing;
using CarRentalApi._2_Modules.Cars._2_Application.UseCases;
using CarRentalApi._2_Modules.Cars._3_Domain.Policies;
using CarRentalApi._2_Modules.Cars._4_Infrastructure.Adapters;
using CarRentalApi._2_Modules.Cars._4_Infrastructure.ReadModel;
using CarRentalApi._2_Modules.Cars._4_Infrastructure.Repositories;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
namespace CarRentalApi._2_Modules.Cars;

public static class DiAddCarsExtensions {
   
   public static IServiceCollection AddCars(
      this IServiceCollection services
   ) {
      
      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // Contracts BC-to-BC
      services.AddScoped<ICarReadContract, CarReadContractServiceEf>();
      services.AddScoped<ICarWriteContract, CarWriteContractServiceEf>();

      // ReadModels (Queries)
      services.AddScoped<ICarReadModel, CarReadModelEf>();
      
      // WriteModels = Use Cases
      services.AddScoped<CarUcCreate>();
      services.AddScoped<CarUcSendToMaintenance>();
      services.AddScoped<CarUcReturnFromMaintenance>();
      services.AddScoped<CarUcRetire>();
      services.AddScoped<ICarUseCases, CarUseCases>();
      
      // Policies
      services.AddScoped<ICarRemovalPolicy, AllowAllCarRemovalPolicy>();

      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<ICarRepository, CarRepositoryEf>();
      
      
      // Pricing Strategies
      services.AddScoped<IPricingPolicyCarCategories, PricingPolicyCarCategories>();
      
      return services;
   }
}