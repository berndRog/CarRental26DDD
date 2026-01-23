namespace CarRentalApi._4_BuildingBlocks._4_Infrastructure.ReadModel;

public sealed record SortRequest(
   string SortBy = "id",
   SortDirection Direction = SortDirection.Asc
);