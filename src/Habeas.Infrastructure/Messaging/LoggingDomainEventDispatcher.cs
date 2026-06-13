using Habeas.Application.Common;
using Habeas.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Habeas.Infrastructure.Messaging;

/// <summary>
/// Minimal dispatcher that logs each domain event. Replace or extend with a real in-process
/// mediator (or an outbox + message bus) once event handlers are introduced.
/// </summary>
internal sealed class LoggingDomainEventDispatcher(ILogger<LoggingDomainEventDispatcher> logger)
    : IDomainEventDispatcher
{
    public Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            logger.LogInformation(
                "Domain event {EventType} occurred at {OccurredAt:o}.",
                domainEvent.GetType().Name,
                domainEvent.OccurredAt);
        }

        return Task.CompletedTask;
    }
}
