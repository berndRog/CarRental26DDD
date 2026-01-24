using CarRentalApi._2_Modules.Bookings;
using CarRentalApi._2_Modules.Cars;
using CarRentalApi._2_Modules.Customers;
using CarRentalApi._2_Modules.Employees;
using CarRentalApi._4_BuildingBlocks;
using CarRentalApi.Data.Extensions;
using CarRentalApi.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
namespace CarRentalApi;

public class Program {
   public static void Main(string[] args) {
      var builder = WebApplication.CreateBuilder(args);

      builder.Services.AddHttpLogging(o => {
         o.LoggingFields =
            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod |
            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath |
            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestQuery |
            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestHeaders |
            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode |
            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseHeaders;

         // optional: Bodies (nur DEV, Achtung: kann sensibel sein)
         o.LoggingFields |= Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestBody |
            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseBody;

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