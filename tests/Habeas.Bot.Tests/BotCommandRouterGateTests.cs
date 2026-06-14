using Habeas.Application.Common;
using Habeas.Application.Users;
using Habeas.Bot.Telegram;
using Habeas.Domain.Common;

namespace Habeas.Bot.Tests;

/// <summary>
/// Covers the registration gate in <see cref="BotCommandRouter"/>: commands other than /start and
/// /help must be refused for users who haven't registered, without touching their use case.
/// </summary>
[TestFixture]
public sealed class BotCommandRouterGateTests
{
    private const long UserId = 123;

    [TestCase("/me")]
    [TestCase("/history")]
    [TestCase("/birth")]
    [TestCase("/body")]
    public async Task RouteAsync_UnregisteredUser_RefusesGatedCommand(string command)
    {
        var getProfile = new StubQueryHandler<GetProfile.Query, GetProfile.ProfileView>();
        var router = CreateRouter(registered: false, getProfile);

        var response = await router.RouteAsync(UserId, "Alice", command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.Text, Does.Contain("Send /start first."));
            // The gate short-circuits before the command's own use case runs.
            Assert.That(getProfile.WasCalled, Is.False);
        });
    }

    [Test]
    public async Task RouteAsync_RegisteredUser_LetsGatedCommandThrough()
    {
        var profile = new GetProfile.ProfileView("Alice", null, null, null);
        var getProfile = new StubQueryHandler<GetProfile.Query, GetProfile.ProfileView>(profile);
        var router = CreateRouter(registered: true, getProfile);

        var response = await router.RouteAsync(UserId, "Alice", "/me", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(getProfile.WasCalled, Is.True);
            Assert.That(response.Text, Does.Contain("Alice"));
        });
    }

    [Test]
    public async Task RouteAsync_UnregisteredUser_AllowsHelp()
    {
        var router = CreateRouter(
            registered: false, new StubQueryHandler<GetProfile.Query, GetProfile.ProfileView>());

        var response = await router.RouteAsync(UserId, "Alice", "/help", CancellationToken.None);

        Assert.That(response.Text, Does.Not.Contain("Send /start first."));
    }

    private static BotCommandRouter CreateRouter(
        bool registered, StubQueryHandler<GetProfile.Query, GetProfile.ProfileView> getProfile) =>
        new(
            new StubCommandHandler<RegisterUser.Command, RegisterUser.Registration>(),
            new StubCommandHandler<SetDateOfBirth.Command, SetDateOfBirth.DateOfBirthView>(),
            new StubCommandHandler<RecordBodyMeasurement.Command, MeasurementView>(),
            getProfile,
            new StubQueryHandler<GetMeasurementHistory.Query, GetMeasurementHistory.HistoryView>(),
            new StubQueryHandler<IsRegistered.Query, bool>(registered),
            new ConversationState(),
            // The menu is only touched on /start, which these tests never exercise.
            new BotMenu(null!));

    /// <summary>
    /// Records whether it ran and returns a preset result; with no result it throws when called,
    /// so tests can assert a handler was never reached.
    /// </summary>
    private sealed class StubQueryHandler<TQuery, TResponse>(Result<TResponse>? result = null)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public bool WasCalled { get; private set; }

        public Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(
                result ?? throw new InvalidOperationException("Handler should not have been called."));
        }
    }

    private sealed class StubCommandHandler<TCommand, TResponse>(Result<TResponse>? result = null)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public bool WasCalled { get; private set; }

        public Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            WasCalled = true;
            return Task.FromResult(
                result ?? throw new InvalidOperationException("Handler should not have been called."));
        }
    }
}
