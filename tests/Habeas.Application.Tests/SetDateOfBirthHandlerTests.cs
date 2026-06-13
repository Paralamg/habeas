using Habeas.Application.Common;
using Habeas.Application.Users;
using Habeas.Domain.Users;

namespace Habeas.Application.Tests;

[TestFixture]
public sealed class SetDateOfBirthHandlerTests
{
    [Test]
    public async Task Handle_UnregisteredUser_ReturnsNotFound()
    {
        var handler = new SetDateOfBirth.Handler(new InMemoryUserRepository(), new NoOpUnitOfWork());

        var result = await handler.Handle(
            new SetDateOfBirth.Command(123, new DateOnly(1990, 5, 20)), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task Handle_RegisteredUser_StoresDateOfBirth()
    {
        var users = new InMemoryUserRepository();
        var user = UserProfile.Register(TelegramUserId.Create(123).Value, "Alice").Value;
        users.All.Add(user);
        var handler = new SetDateOfBirth.Handler(users, new NoOpUnitOfWork());

        var result = await handler.Handle(
            new SetDateOfBirth.Command(123, new DateOnly(1990, 5, 20)), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.DateOfBirth, Is.EqualTo(new DateOnly(1990, 5, 20)));
            Assert.That(user.DateOfBirth, Is.Not.Null);
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
