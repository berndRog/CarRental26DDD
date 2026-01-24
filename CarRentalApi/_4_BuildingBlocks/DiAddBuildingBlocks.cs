using CarRentalApi._4_BuildingBlocks._1_Ports.Inbound;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._4_Infrastructure;
using CarRentalApi._4_BuildingBlocks._4_Infrastructure.Security;
namespace CarRentalApi._4_BuildingBlocks;

public static class DiAddBuildingBlocks {
   
   public static IServiceCollection AddBuildingBlocks(
      this IServiceCollection services
   ) {
      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // ReadModels (Queries)     
      services.AddScoped<IClock, CarSystemClock>();
      
      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<IIdentityGateway, IdentityGatewayHttpContext>();

      return services;
   }
}