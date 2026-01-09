namespace CarRentalApiTest;

using Microsoft.Extensions.Logging;

public static class TestLogger {
   
   public static ILogger<T> Create<T>(bool enabled) {
      
      if (!enabled)
         return Microsoft.Extensions.Logging.Abstractions.NullLogger<T>.Instance;

      var factory = LoggerFactory.Create(b => {
         b.ClearProviders();
         b.AddSimpleConsole(o => {
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss ";
         });
         b.SetMinimumLevel(LogLevel.Debug);
      });

      return factory.CreateLogger<T>();
   }
}
