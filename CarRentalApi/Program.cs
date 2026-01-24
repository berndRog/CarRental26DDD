using CarRentalApi._2_Modules.Bookings;
using CarRentalApi._2_Modules.Cars;
using CarRentalApi._2_Modules.Customers;
using CarRentalApi._2_Modules.Employees;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi.Data.Extensions;
using CarRentalApi.Infrastructure.Security;
using Microsoft.AspNetCore.HttpLogging;
namespace CarRentalApi;

public class Program {
   public static void Main(string[] args) {
      
      var builder = WebApplication.CreateBuilder(args);
      
      builder.Services.AddHttpContextAccessor();

      builder.Services.AddHttpLogging(o => {
         o.LoggingFields =
            HttpLoggingFields.RequestMethod |
            HttpLoggingFields.RequestPath |
            HttpLoggingFields.RequestQuery |
            HttpLoggingFields.RequestHeaders |
            HttpLoggingFields.ResponseStatusCode |
            HttpLoggingFields.ResponseHeaders;

         // optional: Bodies (nur DEV, Achtung: kann sensibel sein)
         o.LoggingFields |= HttpLoggingFields.RequestBody |
            HttpLoggingFields.ResponseBody;

         o.RequestHeaders.Add("Authorization"); // Achtung: Token wird geloggt (DEV ok, PROD nein)
         o.MediaTypeOptions.AddText("application/json");
      });

      // Controllers
      builder.Services.AddControllers();

      // Modules
      builder.Services.AddBookings();
      builder.Services.AddCustomers();
      builder.Services.AddCars();
      builder.Services.AddEmployees();
      builder.Services.AddBuildingBlocks();
      builder.Services.AddInfrastructure(builder.Configuration);
      
      // AuthN (Bearer)
      builder.Services.AddJwtAuthentication(builder.Configuration);
      // AuthZ
      builder.Services.AddAuthorization();
      
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment()) {
         app.UseHttpLogging();
         app.UseDeveloperExceptionPage();

         app.UseSwagger();
         app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();

      app.UseAuthentication();
      app.UseAuthorization();

      app.MapControllers();

      app.Run();
   }
}