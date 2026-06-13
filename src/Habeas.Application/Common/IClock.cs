namespace Habeas.Application.Common;

/// <summary>Abstraction over the system clock so use cases stay deterministic and testable.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
