using Habeas.Domain.Common;

namespace Habeas.Domain.Users;

/// <summary>
/// A body characteristic the bot can track over time (e.g. height, weight). Modelled as a
/// smart enum so each metric carries its own canonical unit and validation range, and new
/// metrics can be added by declaring another instance — buttons and parsing pick them up via
/// <see cref="All"/> and <see cref="FromKey"/>.
/// </summary>
public sealed class MetricType : ValueObject
{
    public static readonly MetricType Height = new("height", "Height", "cm", min: 0, max: 300);
    public static readonly MetricType Weight = new("weight", "Weight", "kg", min: 0, max: 700);

    /// <summary>All known metrics, in display order.</summary>
    public static readonly IReadOnlyList<MetricType> All = [Height, Weight];

    private MetricType(string key, string displayName, string unit, double min, double max)
    {
        Key = key;
        DisplayName = displayName;
        Unit = unit;
        Min = min;
        Max = max;
    }

    /// <summary>Stable identifier used in callback data and persistence.</summary>
    public string Key { get; }
    public string DisplayName { get; }
    public string Unit { get; }
    public double Min { get; }
    public double Max { get; }

    /// <summary>Resolves a metric from its <see cref="Key"/>; returns null if unknown.</summary>
    public static MetricType? FromKey(string key) =>
        All.FirstOrDefault(m => string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase));

    /// <summary>Checks a measured value against this metric's plausible range.</summary>
    public Result Validate(double value) =>
        value > Min && value <= Max
            ? Result.Success()
            : Result.Failure(Error.Validation($"{DisplayName} must be between {Min} and {Max} {Unit}."));

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Key;
    }
}
