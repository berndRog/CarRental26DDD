using CarRentalApi._3_Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CarRentalApi.Infrastructure.Security;

public static class DiAddJwtAuthentication {
   
   public static IServiceCollection AddJwtAuthentication(
      this IServiceCollection services,
      IConfiguration config
   ) {
      services.AddOptions<AuthOptions>()
         .Bind(config.GetSection("AuthServer")) 
         .Validate(o => !string.IsNullOrWhiteSpace(o.Authority), "AuthServer:Authority is required.")
         .ValidateOnStart();

      var auth = config.GetSection("AuthServer").Get<AuthOptions>()
         ?? throw new InvalidOperationException("Missing configuration section 'AuthServer'.");

      Console.WriteLine($"JWT Bearer Authority: {auth.Authority}");
      Console.WriteLine($"JWT Bearer Audience: {auth.Audience}");
      Console.WriteLine($"JWT Bearer ValidateAudience: {auth.ValidateAudience}");
      Console.WriteLine($"JWT Bearer RequireHttpsMetadata: {auth.RequireHttpsMetadata}");
      Console.WriteLine($"JWT Bearer ClockSkewSeconds: {auth.ClockSkewSeconds}");

      // JWT Bearer
      services
         .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(opt => {
            opt.Authority = auth.Authority;
            opt.RequireHttpsMetadata = auth.RequireHttpsMetadata;

            if (!string.IsNullOrWhiteSpace(auth.Audience))
               opt.Audience = auth.Audience;

            opt.TokenValidationParameters = new TokenValidationParameters {
               ValidateAudience = auth.ValidateAudience,
               ClockSkew = TimeSpan.FromSeconds(auth.ClockSkewSeconds)
            };

            // optional aber sehr hilfreich:
            opt.Events = new JwtBearerEvents {
               OnAuthenticationFailed = ctx => {
                  var log = ctx.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");
                  log.LogError(ctx.Exception, "JWT auth failed");
                  return Task.CompletedTask;
               },
               OnChallenge = ctx => {
                  var log = ctx.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");
                  log.LogWarning("JWT challenge: error={Error}, desc={Desc}",
                     ctx.Error, ctx.ErrorDescription);
                  return Task.CompletedTask;
               }
            };
         });

      return services;
   }
}