using Habeas.Domain.Common;

namespace Habeas.Application.Common;

/// <summary>A request that changes state and returns a <see cref="Result{TResponse}"/>.</summary>
public interface ICommand<TResponse>;

/// <summary>A request that reads state without changing it.</summary>
public interface IQuery<TResponse>;

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
