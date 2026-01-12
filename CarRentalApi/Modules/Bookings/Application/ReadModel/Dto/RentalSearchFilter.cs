namespace CarRentalApi.Modules.Rentals.Application.ReadModel.Dto;
public sealed record RentalSearchFilter(
   Guid? CustomerId,
   Guid? CarId,
   DateTimeOffset? From,
   DateTimeOffset? To,
   string? Status
);
