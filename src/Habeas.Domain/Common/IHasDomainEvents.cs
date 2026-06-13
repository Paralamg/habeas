namespace Habeas.Domain.Common;

/// <summary>
/// Non-generic view over an aggregate's recorded domain events, so infrastructure can
/// collect them uniformly regardless of the aggregate's identifier type.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
