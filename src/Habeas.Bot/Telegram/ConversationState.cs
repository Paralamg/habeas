using System.Collections.Concurrent;
using Habeas.Domain.Users;

namespace Habeas.Bot.Telegram;

/// <summary>A reply the bot is currently waiting for from a user, across two-step flows.</summary>
internal abstract record PendingStep;

/// <summary>Waiting for the value of a body metric after the user tapped a <c>/body</c> button.</summary>
internal sealed record AwaitingMeasurement(MetricType MetricType) : PendingStep;

/// <summary>Waiting for the date of birth the user is entering after sending <c>/start</c>.</summary>
internal sealed record AwaitingDateOfBirth : PendingStep;

/// <summary>
/// In-memory record of what each user is currently being asked to enter, bridging the two steps
/// of an interactive flow (prompt → reply). Suitable for the single-instance long-polling bot;
/// a pending prompt is simply lost on restart, which is harmless.
/// </summary>
internal sealed class ConversationState
{
    private readonly ConcurrentDictionary<long, PendingStep> _pending = new();

    /// <summary>Marks that the user has been asked to provide the given input.</summary>
    public void Await(long telegramUserId, PendingStep step) =>
        _pending[telegramUserId] = step;

    /// <summary>Reads and clears the pending step for the user, if any.</summary>
    public bool TryConsume(long telegramUserId, out PendingStep step) =>
        _pending.TryRemove(telegramUserId, out step!);

    /// <summary>Discards any pending prompt for the user.</summary>
    public void Clear(long telegramUserId) => _pending.TryRemove(telegramUserId, out _);
}
