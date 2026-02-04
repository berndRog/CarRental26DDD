namespace CarRentalApiTest;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public static class TestLogger {
   
   private static readonly ILoggerFactory Factory = LoggerFactory.Create(b => {
      b.ClearProviders();
      b.AddSimpleConsole(o =>
      {
         o.SingleLine = false;           // keep real newlines
         o.TimestampFormat = "HH:mm:ss ";
      });
      b.SetMinimumLevel(LogLevel.Debug);
   });

   public static ILogger<T> Create<T>(bool enabled)
      => enabled ? Factory.CreateLogger<T>() : NullLogger<T>.Instance;
}
