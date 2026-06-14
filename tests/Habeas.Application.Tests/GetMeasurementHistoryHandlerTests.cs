using Habeas.Domain.Users;
using Habeas.Application.Users;

namespace Habeas.Application.Tests;

[TestFixture]
public sealed class GetMeasurementHistoryHandlerTests
{
    [Test]
    public async Task Handle_UnregisteredUser_ReturnsNotFound()
    {
        var handler = new GetMeasurementHistory.Handler(new InMemoryUserRepository());

        var result = await handler.Handle(new GetMeasurementHistory.Query(123), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task Handle_ReturnsMeasurementsOldestFirst()
    {
        var users = new InMemoryUserRepository();
        var user = NewUser(123);
        user.RecordMeasurement(MetricType.Weight, 74, At(2026, 6, 13));
        user.RecordMeasurement(MetricType.Weight, 75, At(2026, 6, 1));
        users.All.Add(user);
        var handler = new GetMeasurementHistory.Handler(users);

        var result = await handler.Handle(new GetMeasurementHistory.Query(123), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Measurements, Has.Count.EqualTo(2));
            Assert.That(result.Value.Measurements[0].Value, Is.EqualTo(75));
            Assert.That(result.Value.Measurements[1].Value, Is.EqualTo(74));
        });
    }

    private static UserProfile NewUser(long telegramId) =>
        UserProfile.Register(TelegramUserId.Create(telegramId).Value, "Alice").Value;

    private static DateTimeOffset At(int year, int month, int day) =>
        new(new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc));

    private sealed class InMemoryUserRepository : Habeas.Domain.Users.IUserRepository
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
