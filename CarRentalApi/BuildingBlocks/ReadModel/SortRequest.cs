namespace CarRentalApi.BuildingBlocks.ReadModel;

public sealed record SortRequest(
   string SortBy = "id",
   SortDirection Direction = SortDirection.Asc
);

public enum SortDirection {
   Asc = 1,
   Desc = 2
}