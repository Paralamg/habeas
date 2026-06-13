using Habeas.Domain.Common;

namespace Habeas.Domain.Users.Events;

public sealed record UserRegistered(UserId UserId, long TelegramUserId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
