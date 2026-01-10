namespace CarRentalApi.Modules.Cars.Application.ReadModel.Dto;

/// <summary>
/// Paged result returned by search endpoints.
/// </summary>
public sealed record PagedResult<T>(
   IReadOnlyList<T> Items,
   int PageNumber,
   int PageSize,
   int TotalCount
) {
   public int TotalPages =>
      PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
