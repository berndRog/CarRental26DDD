// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.Extensions.Options;
// using Microsoft.IdentityModel.Tokens;
//
// namespace CarRentalApi.Infrastructure.Security;
//
// public static class JwtBearerSetup {
//    public static IServiceCollection AddJwtAuthentication(
//       this IServiceCollection services,
//       IConfiguration config
//    ) {
//       services
//          .AddOptions<AuthOptions>()
//          .Bind(config.GetSection("Auth"))
//          .ValidateOnStart();
//
//       services
//          .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//          .AddJwtBearer((sp, options) =>
//          {
//             var auth = sp.GetRequiredService<IOptions<AuthOptions>>().Value;
//
//             options.Authority = auth.Authority;
//             options.RequireHttpsMetadata = auth.RequireHttpsMetadata;
//
//             if (!string.IsNullOrWhiteSpace(auth.Audience))
//                options.Audience = auth.Audience;
//
//             options.TokenValidationParameters = new TokenValidationParameters
//             {
//                ValidateAudience = auth.ValidateAudience,
//                ClockSkew = TimeSpan.FromSeconds(auth.ClockSkewSeconds)
//             };
//          });
//
//       services.AddAuthorization();
//       return services;
//    }
// }
