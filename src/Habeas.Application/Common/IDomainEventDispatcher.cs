using Habeas.Domain.Common;

namespace Habeas.Application.Common;

/// <summary>
/// Publishes domain events after the unit of work commits, so side effects (e.g. updating
/// read models, notifying the user) happen outside the aggregate's transaction boundary.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
