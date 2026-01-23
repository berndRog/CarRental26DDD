using CarRentalApi._2_Modules.Employees._2_Application.UseCases;
using CarRentalApi._2_Modules.Employees._4_Infrastructure.Repositories;
using CarRentalApi.Modules.Employees.Domain;
using CarRentalApi.Modules.Employees.Ports.Inbound;
namespace CarRentalApi._2_Modules.Employees;

public static class DiAddEmployeesExtensions {

   public static IServiceCollection AddEmployees(
      this IServiceCollection services
   ) {

      // =========================================================
      // Inbound ports (HTTP / UI)
      // =========================================================
      // Contracts      
      //services.AddScoped<ICustomerReadContract, CustomerReadContractServiceEf>();

      // ReadModels (Queries)
      //services.AddScoped<IEmployeeReadModel, EmployeeReadModelEf>();

      // WriteModels = Use Cases
      services.AddScoped<EmployeeUcCreate>();
      services.AddScoped<EmployeeUcDeactivate>();
      services.AddScoped<EmployeeUcSetAdminRights>();
      services.AddScoped<IEmployeeUseCases, EmployeeUseCases>();

      // Policies

      // =========================================================
      // Outbound ports
      // =========================================================
      // Repositories
      services.AddScoped<IEmployeeRepository, EmployeeRepositoryEf>();

      return services;
   }
}