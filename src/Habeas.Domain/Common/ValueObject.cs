namespace Habeas.Domain.Common;

/// <summary>
/// Base class for value objects — immutable types with no identity whose equality
/// is structural (defined by the components they are made of).
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(default(HashCode), (hash, component) =>
            {
                hash.Add(component);
                return hash;
            })
            .ToHashCode();
}
