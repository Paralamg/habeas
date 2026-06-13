namespace Habeas.Domain.Common;

/// <summary>
/// Base class for entities — objects with a stable identity that is preserved
/// across state changes. Equality is defined by identity, not by attribute values.
/// </summary>
public abstract class Entity<TId>
    where TId : notnull
{
    protected Entity(TId id) => Id = id;

    public TId Id { get; protected init; }

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && GetType() == other.GetType() && Id.Equals(other.Id);

    public override int GetHashCode() => Id.GetHashCode();
}
