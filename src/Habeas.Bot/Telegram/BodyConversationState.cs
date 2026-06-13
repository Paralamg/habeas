using System.Collections.Concurrent;
using Habeas.Domain.Users;

namespace Habeas.Bot.Telegram;

/// <summary>
/// In-memory record of which metric each user is currently being asked to enter, bridging the
/// two steps of the <c>/body</c> flow (button tap → value reply). Suitable for the single-instance
/// long-polling bot; a pending prompt is simply lost on restart, which is harmless.
/// </summary>
internal sealed class BodyConversationState
{
    private readonly ConcurrentDictionary<long, MetricType> _awaiting = new();

    /// <summary>Marks that the user has been asked to enter a value for the given metric.</summary>
    public void Await(long telegramUserId, MetricType metricType) =>
        _awaiting[telegramUserId] = metricType;

    /// <summary>Reads and clears the pending metric for the user, if any.</summary>
    public bool TryConsume(long telegramUserId, out MetricType metricType) =>
        _awaiting.TryRemove(telegramUserId, out metricType!);

    /// <summary>Discards any pending prompt for the user.</summary>
    public void Clear(long telegramUserId) => _awaiting.TryRemove(telegramUserId, out _);
}
