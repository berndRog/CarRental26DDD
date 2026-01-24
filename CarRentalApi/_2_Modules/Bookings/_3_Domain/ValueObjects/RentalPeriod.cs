using CarRentalApi._2_Modules.Bookings._3_Domain.Errors;
using CarRentalApi._4_BuildingBlocks;
namespace CarRentalApi._2_Modules.Bookings._3_Domain.ValueObjects;

public sealed record class RentalPeriod {
   
   public DateTimeOffset Start { get; init; }
   public DateTimeOffset End { get; init; }
   
   private RentalPeriod(DateTimeOffset start, DateTimeOffset end) {
      Start = start;
      End = end;
   }
   
   public static Result<RentalPeriod> Create(
      DateTimeOffset start, 
      DateTimeOffset end
   ) {
      
      // Domain invariant:
      // A rental period must have a positive duration.
      if (start >= end)
         return Result<RentalPeriod>.Failure(ReservationErrors.InvalidPeriod);

      return Result<RentalPeriod>.Success(new RentalPeriod(start, end));
   }

   // Half-open interval overlap check:
   // [Start, End) overlaps [other.Start, other.End)
   public bool Overlaps(RentalPeriod other) =>
      Start < other.End && other.Start < End;
}
