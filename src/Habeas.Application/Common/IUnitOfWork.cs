namespace Habeas.Application.Common;

/// <summary>
/// Commits all changes tracked within a single business transaction and dispatches the
/// domain events recorded by the affected aggregates.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
