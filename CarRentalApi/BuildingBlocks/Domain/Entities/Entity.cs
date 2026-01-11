namespace CarRentalApi.BuildingBlocks.Domain.Entities;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
   where TId : notnull {
   
   // id == primary key
   public TId Id { get; protected set; } = default!;

   // override equality based on ReservationId
   public override bool Equals(object? obj) {
      
      if (obj is not Entity<TId> other)
         return false;

      if (ReferenceEquals(this, other))
         return true;

      // Transient entities (ReservationId not set yet) are never equal
      if (Equals(Id, default(TId)) || Equals(other.Id, default(TId)))
         return false;

      return EqualityComparer<TId>.Default.Equals(Id, other.Id);
   }

   // IEquatable implementation
   public bool Equals(Entity<TId>? other) =>
      Equals((object?)other);

   // override GetHashCode
   public override int GetHashCode() =>
      EqualityComparer<TId>.Default.GetHashCode(Id);

   // overload == and != operators
   public static bool operator ==(Entity<TId>? a, Entity<TId>? b) {
      if (a is null && b is null) return true;
      if (a is null || b is null) return false;
      return a.Equals(b);
   }

   // overload != operator
   public static bool operator !=(Entity<TId>? a, Entity<TId>? b) =>
      !(a == b);
}