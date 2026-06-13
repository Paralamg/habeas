namespace Habeas.Application.Users;

/// <summary>
/// Read model for a user's current body metrics: the latest height and weight recorded, plus
/// the derived BMI. Any field is null until its underlying measurement exists.
/// </summary>
public sealed record BodyMetricsView(double? HeightCm, double? WeightKg, double? Bmi);

/// <summary>Read model returned after a single measurement is recorded.</summary>
public sealed record MeasurementView(string Metric, double Value, string Unit, DateTimeOffset RecordedAt);
