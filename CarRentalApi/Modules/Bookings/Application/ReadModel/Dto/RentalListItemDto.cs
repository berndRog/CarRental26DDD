namespace CarRentalApi.Modules.Rentals.Application.ReadModel.Dto;

public sealed record RentalListItemDto(
   Guid RentalId,
   Guid CarId,
   DateTimeOffset Start,
   DateTimeOffset End,
   string Status,
   DateTimeOffset CreatedAt
);
