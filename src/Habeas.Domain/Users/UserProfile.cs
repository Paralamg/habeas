using Habeas.Domain.Common;
using Habeas.Domain.Users.Events;

namespace Habeas.Domain.Users;

/// <summary>
/// Aggregate root for a person known to the bot: who they are in Telegram and their
/// body measurements over time (height/weight). This is deliberately the minimal core — add
/// new profile facts and behaviours as separate methods/value objects as the bot grows.
/// </summary>
public sealed class UserProfile : AggregateRoot<UserId>
{
    private readonly List<BodyMeasurement> _measurements = [];

    // Required by EF Core's materialization.
    private UserProfile(UserId id) : base(id) => TelegramUserId = null!;

    private UserProfile(UserId id, TelegramUserId telegramUserId, string displayName, DateOfBirth dateOfBirth)
        : base(id)
    {
        TelegramUserId = telegramUserId;
        DisplayName = displayName;
        DateOfBirth = dateOfBirth;
        CreatedAt = DateTimeOffset.UtcNow;
        Raise(new UserRegistered(id, telegramUserId.Value));
    }

    public TelegramUserId TelegramUserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public DateOfBirth DateOfBirth { get; private set; } = null!;

    /// <summary>The full history of recorded body measurements, oldest-to-newest is not guaranteed.</summary>
    public IReadOnlyCollection<BodyMeasurement> Measurements => _measurements.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Result<UserProfile> Register(
        TelegramUserId telegramUserId, string displayName, DateOfBirth dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Error.Validation("Display name is required.");
        }

        return new UserProfile(UserId.New(), telegramUserId, displayName.Trim(), dateOfBirth);
    }

    /// <summary>Appends a new measurement for the given metric, validated against its range.</summary>
    public Result RecordMeasurement(MetricType metricType, double value, DateTimeOffset recordedAt)
    {
        var validation = metricType.Validate(value);
        if (validation.IsFailure)
        {
            return validation;
        }

        _measurements.Add(BodyMeasurement.Create(metricType, value, recordedAt));
        Touch();
        return Result.Success();
    }

    /// <summary>The most recent measurement of the given metric, or null if none recorded.</summary>
    public BodyMeasurement? LatestOf(MetricType metricType) =>
        _measurements
            .Where(m => m.MetricType == metricType)
            .MaxBy(m => m.RecordedAt);

    /// <summary>
    /// Body Mass Index from the latest height and weight; null until both have been recorded.
    /// </summary>
    public double? CurrentBmi
    {
        get
        {
            var height = LatestOf(MetricType.Height);
            var weight = LatestOf(MetricType.Weight);
            if (height is null || weight is null)
            {
                return null;
            }

            return weight.Value / Math.Pow(height.Value / 100d, 2);
        }
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
