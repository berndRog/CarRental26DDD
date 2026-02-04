using CarRentalApi._4_BuildingBlocks.Infrastructure.Persistence;
namespace CarRentalApi._3_Infrastructure.Persistence.Database;

public sealed class UnitOfWork(
   CarRentalDbContext _dbContext,
   ILogger<UnitOfWork> _logger
) : IUnitOfWork {
   public async Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ctToken = default
   ) {
      var nl = Environment.NewLine;
      _dbContext.ChangeTracker.DetectChanges();
      if (_logger.IsEnabled(LogLevel.Debug)) {
         if (!string.IsNullOrWhiteSpace(text))
            _logger.LogDebug("{Text}", text);
         _logger.LogDebug("{Message}", 
            "Before SaveChanges" + Environment.NewLine +
            _dbContext.ChangeTracker.DebugView.LongView);
      }

      var rows = await _dbContext.SaveChangesAsync(ctToken);

      if (_logger.IsEnabled(LogLevel.Debug)) {
         _logger.LogDebug("SaveChanges affected {Result} rows", rows);
         _logger.LogDebug("{Message}", 
            "After SaveChanges" + Environment.NewLine +
            _dbContext.ChangeTracker.DebugView.LongView);
      }
      
      return rows;
   }

   public void ClearChangeTracker() =>
      _dbContext.ChangeTracker.Clear();

   public void LogChangeTracker(string text) {
      if (_logger.IsEnabled(LogLevel.Debug)) {
         _dbContext.ChangeTracker.DetectChanges();
         _logger.LogDebug("{Message}", 
            text + Environment.NewLine +
            _dbContext.ChangeTracker.DebugView.LongView);
      }
   }
}
