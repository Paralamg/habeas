namespace Habeas.Domain.Common;

/// <summary>
/// Marker for something meaningful that happened in the domain. Domain events are
/// raised by aggregates and handled by the application layer after persistence.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
