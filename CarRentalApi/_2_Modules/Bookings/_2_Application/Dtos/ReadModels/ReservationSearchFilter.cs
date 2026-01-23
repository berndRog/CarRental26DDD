using CarRentalApi._2_Modules.Bookings._3_Domain.Enums;
using CarRentalApi._4_BuildingBlocks._3_Domain.Enums;
namespace CarRentalApi._2_Modules.Bookings._2_Application.Dtos.ReadModels;

public sealed class ReservationSearchFilter {
   public Guid? CustomerId { get; init; }
   public CarCategory? CarCategory { get; init; }
   public ReservationStatus? ReservationStatus { get; init; }
   public DateTimeOffset? From { get; init; }
   public DateTimeOffset? To { get; init; }
}
