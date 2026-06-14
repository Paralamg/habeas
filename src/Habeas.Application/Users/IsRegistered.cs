using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users;

public class IsRegistered
{
    /// <summary>Checks whether a Telegram user has completed registration via /start.</summary>
    public sealed record Query(long TelegramUserId) : IQuery<bool>;

    public sealed class Handler(IUserRepository users) : IQueryHandler<Query, bool>
    {
        public async Task<Result<bool>> Handle(Query query, CancellationToken cancellationToken) =>
            await users.ExistsByTelegramIdAsync(query.TelegramUserId, cancellationToken);
    }
}
