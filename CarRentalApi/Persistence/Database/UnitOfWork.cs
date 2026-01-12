using CarRentalApi.BuildingBlocks.Persistence;
using CarRentalApi.Domain;
namespace CarRentalApi.Data.Database;

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
         _logger.LogDebug("{Text}\n{View}", text ?? "Before SaveChanges",
            _dbContext.ChangeTracker.DebugView.LongView);
      }

      var rows = await _dbContext.SaveChangesAsync(ctToken);

      if (_logger.IsEnabled(LogLevel.Debug)) {
         _logger.LogDebug("\nSaveChanges affected {Result} rows", rows);
         _logger.LogDebug("\nAfter SaveChanges\n{View}", _dbContext.ChangeTracker.DebugView.LongView);
      }

      return rows;
   }

   public void ClearChangeTracker() =>
      _dbContext.ChangeTracker.Clear();

   public void LogChangeTracker(string text) {
      if (_logger.IsEnabled(LogLevel.Debug)) {
         _dbContext.ChangeTracker.DetectChanges(); 
         _logger.LogDebug("{Text}\n{Change}", text, _dbContext.ChangeTracker.DebugView.LongView);
      }
   }
}