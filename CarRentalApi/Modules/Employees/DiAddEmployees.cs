using CarRentalApi.Modules.Employees.Infrastructure;
using CarRentalApi.Modules.Employees.Infrastructure.Repositories;
namespace CarRentalApi.Modules.Employees;

public static class DiAddEmployeesExtensions {
   
   public static IServiceCollection AddEmployees(
      this IServiceCollection services
   ) {
      
      services.AddScoped<IEmployeeRepository, EmployeeRepository>();
      return services;
   }
}