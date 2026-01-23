namespace CarRentalApi._2_Modules.Bookings._2_Application.Dtos.ReadModels;

public sealed record RentalListItemDto(
   Guid RentalId,
   Guid CarId,
   DateTimeOffset Start,
   DateTimeOffset End,
   string Status,
   DateTimeOffset CreatedAt
);
