using CarRentalApi.Modules.Cars.Application;
using CarRentalApi.Modules.Cars.Application.Contracts;
using CarRentalApi.Modules.Cars.Application.ReadModel;
using CarRentalApi.Modules.Cars.Application.UseCases;
using CarRentalApi.Modules.Cars.Domain;
using CarRentalApi.Modules.Cars.Domain.Policies;
using CarRentalApi.Modules.Cars.Infrastructure.Adapters;
using CarRentalApi.Modules.Cars.Infrastructure.ReadModel;
using CarRentalApi.Modules.Cars.Infrastructure.Repositories;
using CarRentalApi.Modules.Cars.Ports.Inbound;
using CarRentalApi.Modules.Cars.Ports.Outbound;
namespace CarRentalApi.Modules.Cars;

public static class DiAddCarsExtensions {
   
   public static IServiceCollection AddCars(
      this IServiceCollection services
   ) {
      
      // =========================================================      
      // Contracts BC-to-BC
      // =========================================================
      services.AddScoped<ICarReadContract, CarReadContractServiceEf>();
      services.AddScoped<ICarWriteContract, CarWriteContractServiceEf>();
      
      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
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
      
      return services;
   }
}