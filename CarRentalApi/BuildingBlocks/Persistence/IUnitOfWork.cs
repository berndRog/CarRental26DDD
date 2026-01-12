namespace CarRentalApi.BuildingBlocks.Persistence;

public interface IUnitOfWork {
   Task<int> SaveAllChangesAsync(
      string? text = null,
      CancellationToken ctToken = default
   ); 
   void ClearChangeTracker();
   void LogChangeTracker(string text);
}