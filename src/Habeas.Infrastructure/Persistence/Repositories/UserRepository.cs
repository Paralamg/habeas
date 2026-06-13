using Habeas.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Habeas.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(HabeasDbContext context) : IUserRepository
{
    public Task<UserProfile?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<UserProfile?> GetByTelegramIdAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        var id = TelegramUserId.FromTrusted(telegramUserId);
        return context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == id, cancellationToken);
    }

    public Task<bool> ExistsByTelegramIdAsync(long telegramUserId, CancellationToken cancellationToken = default)
    {
        var id = TelegramUserId.FromTrusted(telegramUserId);
        return context.Users.AnyAsync(u => u.TelegramUserId == id, cancellationToken);
    }

    public async Task AddAsync(UserProfile aggregate, CancellationToken cancellationToken = default) =>
        await context.Users.AddAsync(aggregate, cancellationToken);

    public void Update(UserProfile aggregate) => context.Users.Update(aggregate);
    public void Remove(UserProfile aggregate) => context.Users.Remove(aggregate);
}
