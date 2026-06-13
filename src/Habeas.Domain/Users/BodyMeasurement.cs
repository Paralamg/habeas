using Habeas.Domain.Common;

namespace Habeas.Domain.Users;

/// <summary>
/// A single recorded value for one <see cref="MetricType"/> at a point in time. Measurements
/// form a time series owned by <see cref="UserProfile"/>, so the bot can track how a user's
/// body characteristics change. The unit is not stored — it is canonical per metric.
/// </summary>
public sealed class BodyMeasurement : Entity<BodyMeasurementId>
{
    // Required by EF Core's materialization.
    private BodyMeasurement(BodyMeasurementId id) : base(id) => MetricType = null!;

    private BodyMeasurement(BodyMeasurementId id, MetricType metricType, double value, DateTimeOffset recordedAt)
        : base(id)
    {
        MetricType = metricType;
        Value = value;
        RecordedAt = recordedAt;
    }

    public MetricType MetricType { get; private set; }
    public double Value { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }

    /// <summary>Created via <see cref="UserProfile.RecordMeasurement"/>, which validates the value.</summary>
    internal static BodyMeasurement Create(MetricType metricType, double value, DateTimeOffset recordedAt) =>
        new(BodyMeasurementId.New(), metricType, value, recordedAt);
}
