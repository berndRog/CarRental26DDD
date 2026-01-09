using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Data.Database;
using Microsoft.EntityFrameworkCore;
namespace CarRentalApi.Data.Extensions;

public static class DiAddDataExtensions {
   public static IServiceCollection AddData(
      this IServiceCollection services,
      IConfiguration configuration
   ) {
      // Connection String aus appsettings.json lesen und Pfad erstellen
      var projectDir = Directory.GetCurrentDirectory();
      if (string.IsNullOrEmpty(projectDir))
         throw new InvalidOperationException("Could not determine current directory");
      var dbFile = configuration.GetConnectionString("CarRentalApi"); 
      if(string.IsNullOrEmpty(dbFile)) 
         throw new Exception("ConnectionString for <CarRentalApi> not found in appSettings.json");
      var dbPath = Path.Combine(projectDir, dbFile);
      var connectionString = $"Data Source={dbPath}";
      Console.WriteLine("---> Using SQLite connection string: " + dbPath);
      
      services.AddDbContext<CarRentalDbContext>(options =>
         options.UseSqlite(
            configuration.GetConnectionString("BankingDb"))
      );

      // Unit of Work
      services.AddScoped<IUnitOfWork, UnitOfWork>();
      
      return services;
   }
}