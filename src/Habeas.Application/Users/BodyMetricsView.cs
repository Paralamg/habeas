namespace Habeas.Application.Users;

/// <summary>Read model for a user's body metrics, including the derived BMI.</summary>
public sealed record BodyMetricsView(double HeightCm, double WeightKg, double Bmi);
