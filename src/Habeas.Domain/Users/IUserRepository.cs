using Habeas.Domain.Common;

namespace Habeas.Domain.Users;

public interface IUserRepository : IRepository<UserProfile, UserId>
{
    Task<UserProfile?> GetByTelegramIdAsync(long telegramUserId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByTelegramIdAsync(long telegramUserId, CancellationToken cancellationToken = default);
}
