using CarRentalApi._2_Modules.Bookings._2_Application.UseCases.Rental;
using CarRentalApi._2_Modules.Bookings._2_Application.UseCases.Reservation;
using CarRentalApi._2_Modules.Bookings._3_Domain.Policies;
using CarRentalApi._2_Modules.Bookings._4_Infrastructure.Policies;
using CarRentalApi._2_Modules.Bookings._4_Infrastructure.ReadModel;
using CarRentalApi._2_Modules.Bookings._4_Infrastructure.Repositories;
using CarRentalApi._4_BuildingBlocks._1_Ports.Inbound;
using CarRentalApi._4_BuildingBlocks._1_Ports.Outbound;
using CarRentalApi._4_BuildingBlocks._4_Infrastructure;
using CarRentalApi._4_BuildingBlocks._4_Infrastructure.Security;
using CarRentalApi.Modules.Bookings;
using CarRentalApi.Modules.Bookings.Application.ReadModel;
using CarRentalApi.Modules.Bookings.Domain;
using CarRentalApi.Modules.Rentals.Application.UseCases;
using Microsoft.Extensions.Internal;
namespace CarRentalApi._2_Modules.Bookings;

public static class DiAddBuildingBlocks {
   
   public static IServiceCollection AddBookings(
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