namespace Habeas.Domain.Common;

/// <summary>
/// A domain or application error described by a stable code and a human message.
/// Returned via <see cref="Result"/> instead of throwing for expected rule violations.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Validation(string message) => new("validation", message);
    public static Error NotFound(string message) => new("not_found", message);
    public static Error Conflict(string message) => new("conflict", message);
}
