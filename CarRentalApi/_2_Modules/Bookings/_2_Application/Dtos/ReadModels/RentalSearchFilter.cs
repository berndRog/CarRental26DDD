namespace CarRentalApi._2_Modules.Bookings._2_Application.Dtos.ReadModels;
public sealed record RentalSearchFilter(
   Guid? CustomerId,
   Guid? CarId,
   DateTimeOffset? From,
   DateTimeOffset? To,
   string? Status
);
