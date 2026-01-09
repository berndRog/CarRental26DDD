using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CarRentalApi.Data.Database;

public class CarRentalDbContextFactory : IDesignTimeDbContextFactory<CarRentalDbContext> {
   public CarRentalDbContext CreateDbContext(string[] args) {
      
      var configuration = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json", optional: false)
         .AddJsonFile("appsettings.Development.json", optional: true)
         .Build();

      var projectDir = Directory.GetCurrentDirectory();
      if (string.IsNullOrEmpty(projectDir))
         throw new InvalidOperationException("Could not determine current directory");
      var dbFile = configuration.GetConnectionString("CarRentalApi");
      if(string.IsNullOrEmpty(dbFile)) 
         throw new Exception("ConnectionString for <CarRentalApi> not found in appSettings.json");
      var dbPath = Path.Combine(projectDir, dbFile);
      var connectionString = $"Data Source={dbPath}";
      
      Console.WriteLine("---> Using SQLite connection string: " + dbPath);
      
      var optionsBuilder = new DbContextOptionsBuilder<CarRentalDbContext>();
        
      // Passen Sie den Connection String an Ihre Umgebung an
      optionsBuilder.UseSqlite(connectionString);
      // Oder für SQL Server:
      // optionsBuilder.UseSqlServer("Server=localhost;Database=banking_dev;Trusted_Connection=True;");
        
      return new CarRentalDbContext(optionsBuilder.Options);
   }
}
