namespace Habeas.Domain.Common;

/// <summary>
/// Marker for an aggregate repository. Each aggregate root gets its own repository
/// interface defined in the domain; persistence details live in the infrastructure.
/// </summary>
public interface IRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
    void Update(TAggregate aggregate);
    void Remove(TAggregate aggregate);
}
