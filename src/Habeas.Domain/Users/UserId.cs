namespace Habeas.Domain.Users;

/// <summary>Strongly-typed identifier for a <see cref="UserProfile"/>.</summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}
