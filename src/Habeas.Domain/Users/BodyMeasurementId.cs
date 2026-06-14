namespace Habeas.Domain.Users;

/// <summary>Strongly-typed identifier for a <see cref="BodyMeasurement"/>.</summary>
public readonly record struct BodyMeasurementId(Guid Value)
{
    public static BodyMeasurementId New() => new(Guid.CreateVersion7());
    public override string ToString() => Value.ToString();
}
