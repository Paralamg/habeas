using Habeas.Domain.Common;

namespace Habeas.Domain.Users;

/// <summary>
/// The user's identifier in Telegram. This is how an inbound update is mapped to a
/// domain <see cref="UserProfile"/>; it is stable and unique per Telegram account.
/// </summary>
public sealed class TelegramUserId : ValueObject
{
    private TelegramUserId(long value) => Value = value;

    public long Value { get; }

    public static Result<TelegramUserId> Create(long value) =>
        value > 0
            ? new TelegramUserId(value)
            : Error.Validation("Telegram user id must be a positive number.");

    /// <summary>
    /// Reconstructs the value object from already-validated persisted data. For use by the
    /// persistence layer only — application code must go through <see cref="Create"/>.
    /// </summary>
    public static TelegramUserId FromTrusted(long value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
