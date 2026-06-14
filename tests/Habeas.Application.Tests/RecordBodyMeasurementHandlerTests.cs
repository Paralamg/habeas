using Habeas.Application.Common;
using Habeas.Application.Users;
using Habeas.Domain.Users;

namespace Habeas.Application.Tests;

[TestFixture]
public sealed class RecordBodyMeasurementHandlerTests
{
    [Test]
    public async Task Handle_UnregisteredUser_ReturnsNotFound()
    {
        var handler = new RecordBodyMeasurement.Handler(
            new InMemoryUserRepository(), new NoOpUnitOfWork(), new FixedClock());

        var result = await handler.Handle(
            new RecordBodyMeasurement.Command(123, "height", 180), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task Handle_UnknownMetric_ReturnsValidationError()
    {
        var users = new InMemoryUserRepository();
        users.All.Add(NewUser(123));
        var handler = new RecordBodyMeasurement.Handler(users, new NoOpUnitOfWork(), new FixedClock());

        var result = await handler.Handle(
            new RecordBodyMeasurement.Command(123, "bodyfat", 12), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task Handle_ValidMeasurement_RecordsAndReturnsView()
    {
        var users = new InMemoryUserRepository();
        var user = NewUser(123);
        users.All.Add(user);
        var handler = new RecordBodyMeasurement.Handler(users, new NoOpUnitOfWork(), new FixedClock());

        var result = await handler.Handle(
            new RecordBodyMeasurement.Command(123, "weight", 75), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Metric, Is.EqualTo(MetricType.Weight.DisplayName));
            Assert.That(result.Value.Value, Is.EqualTo(75));
            Assert.That(result.Value.Unit, Is.EqualTo("kg"));
            Assert.That(user.Measurements, Has.Count.EqualTo(1));
        });
    }

    private static UserProfile NewUser(long telegramId) =>
        UserProfile.Register(TelegramUserId.Create(telegramId).Value, "Alice").Value;

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 6, 13, 0, 0, 0, TimeSpan.Zero);
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
