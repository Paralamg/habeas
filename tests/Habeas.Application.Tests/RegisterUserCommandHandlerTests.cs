using Habeas.Application.Common;
using Habeas.Application.Users;
using Habeas.Domain.Users;

namespace Habeas.Application.Tests;

[TestFixture]
public sealed class RegisterUserCommandHandlerTests
{
    [Test]
    public async Task Handle_NewUser_PersistsAndReturnsId()
    {
        var users = new InMemoryUserRepository();
        var handler = new RegisterUser.Handler(users, new NoOpUnitOfWork());

        var result = await handler.Handle(new RegisterUser.Command(123, "Alice"), CancellationToken.None);
        var stored = await users.GetByTelegramIdAsync(123);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(stored, Is.Not.Null);
        });
    }

    [Test]
    public async Task Handle_ExistingUser_IsIdempotent()
    {
        var users = new InMemoryUserRepository();
        var handler = new RegisterUser.Handler(users, new NoOpUnitOfWork());

        var first = await handler.Handle(new RegisterUser.Command(123, "Alice"), CancellationToken.None);
        var second = await handler.Handle(new RegisterUser.Command(123, "Alice again"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(second.Value, Is.EqualTo(first.Value));
            Assert.That(users.All, Has.Count.EqualTo(1));
        });
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        public List<UserProfile> All { get; } = [];

        public Task<UserProfile?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default) =>
            Task.FromResult(All.FirstOrDefault(u => u.Id == id));

        public Task<UserProfile?> GetByTelegramIdAsync(long telegramUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult(All.FirstOrDefault(u => u.TelegramUserId.Value == telegramUserId));

        public Task<bool> ExistsByTelegramIdAsync(long telegramUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult(All.Any(u => u.TelegramUserId.Value == telegramUserId));

        public Task AddAsync(UserProfile aggregate, CancellationToken cancellationToken = default)
        {
            All.Add(aggregate);
            return Task.CompletedTask;
        }

        public void Update(UserProfile aggregate) { }
        public void Remove(UserProfile aggregate) => All.Remove(aggregate);
    }
}
