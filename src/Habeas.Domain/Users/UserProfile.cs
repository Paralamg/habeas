using Habeas.Domain.Common;
using Habeas.Domain.Users.Events;

namespace Habeas.Domain.Users;

/// <summary>
/// Aggregate root for a person known to the bot: who they are in Telegram and their
/// latest body metrics (height/weight). This is deliberately the minimal core — add new
/// profile facts and behaviours as separate methods/value objects as the bot grows.
/// </summary>
public sealed class UserProfile : AggregateRoot<UserId>
{
    // Required by EF Core's materialization.
    private UserProfile(UserId id) : base(id) => TelegramUserId = null!;

    private UserProfile(UserId id, TelegramUserId telegramUserId, string displayName) : base(id)
    {
        TelegramUserId = telegramUserId;
        DisplayName = displayName;
        CreatedAt = DateTimeOffset.UtcNow;
        Raise(new UserRegistered(id, telegramUserId.Value));
    }

    public TelegramUserId TelegramUserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public BodyMetrics? BodyMetrics { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Result<UserProfile> Register(TelegramUserId telegramUserId, string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Error.Validation("Display name is required.");
        }

        return new UserProfile(UserId.New(), telegramUserId, displayName.Trim());
    }

    /// <summary>Records the user's latest body metrics (height and weight).</summary>
    public Result SetBodyMetrics(BodyMetrics metrics)
    {
        BodyMetrics = metrics;
        Touch();
        return Result.Success();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
