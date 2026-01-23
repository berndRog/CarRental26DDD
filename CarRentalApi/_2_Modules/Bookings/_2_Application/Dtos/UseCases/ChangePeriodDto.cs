namespace CarRentalApi._2_Modules.Bookings._2_Application.Dtos.UseCases;

public sealed record ChangePeriodDto(
   DateTimeOffset NewStart,
   DateTimeOffset NewEnd
);
