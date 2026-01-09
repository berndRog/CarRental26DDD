using CarRentalApi.BuildingBlocks.Errors;
using CarRentalApi.BuildingBlocks.Utils;
namespace CarRentalApi.BuildingBlocks.Domain.Entities;

// Shared id generation/parsing for all entities.
// Keeps factories consistent and avoids copy/paste across the domain model.
public static class EntityId {
   
   // If `rawId` is null/empty -> generate a new Guid.
   // If provided -> parse or return `invalidIdError` on failure.
   public static Result<Guid> Resolve(string? rawId, DomainErrors invalidIdError) {
      if (string.IsNullOrWhiteSpace(rawId))
         return Result<Guid>.Success(Guid.NewGuid());

      var guidResult = rawId.ToResultGuid();
      if (guidResult.IsFailure)
         return Result<Guid>.Failure(invalidIdError);

      return Result<Guid>.Success(guidResult.Value!);
   }
}
