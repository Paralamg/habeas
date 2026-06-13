using Habeas.Application.Common;
using Habeas.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Habeas.Infrastructure.Persistence;

/// <summary>
/// Wraps a single <see cref="HabeasDbContext"/> save in a transactional boundary: it
/// persists tracked aggregates, then dispatches the domain events they recorded.
/// </summary>
internal sealed class UnitOfWork(HabeasDbContext context, IDomainEventDispatcher dispatcher) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = CollectDomainEvents();
        var affected = await context.SaveChangesAsync(cancellationToken);
        await dispatcher.DispatchAsync(domainEvents, cancellationToken);
        return affected;
    }

    private IReadOnlyCollection<IDomainEvent> CollectDomainEvents()
    {
        var aggregates = context.ChangeTracker
            .Entries()
            .Select(entry => entry.Entity)
            .OfType<IHasDomainEvents>()
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var events = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());
        return events;
    }
}
