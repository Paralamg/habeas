using Habeas.Domain.Common;

namespace Habeas.Domain.Users;

/// <summary>Basic anthropometric measurements used by recommendation logic.</summary>
public sealed class BodyMetrics : ValueObject
{
    private BodyMetrics(double heightCm, double weightKg)
    {
        HeightCm = heightCm;
        WeightKg = weightKg;
    }

    public double HeightCm { get; }
    public double WeightKg { get; }

    /// <summary>Body Mass Index, derived from the stored measurements.</summary>
    public double Bmi => WeightKg / Math.Pow(HeightCm / 100d, 2);

    public static Result<BodyMetrics> Create(double heightCm, double weightKg)
    {
        if (heightCm is <= 0 or > 300)
        {
            return Error.Validation("Height must be between 0 and 300 cm.");
        }

        if (weightKg is <= 0 or > 700)
        {
            return Error.Validation("Weight must be between 0 and 700 kg.");
        }

        return new BodyMetrics(heightCm, weightKg);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return HeightCm;
        yield return WeightKg;
    }
}
