using Habeas.Domain.Users;
using Habeas.Domain.Users.Events;

namespace Habeas.Domain.Tests;

[TestFixture]
public sealed class UserProfileTests
{
    private static TelegramUserId AnyTelegramId() => TelegramUserId.Create(42).Value;

    private static DateOfBirth AnyDateOfBirth() =>
        DateOfBirth.Create(new DateOnly(1990, 5, 20)).Value;

    [Test]
    public void Register_WithBlankName_Fails()
    {
        var result = UserProfile.Register(AnyTelegramId(), "   ", AnyDateOfBirth());

        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public void Register_Succeeds_AndRaisesUserRegistered()
    {
        var result = UserProfile.Register(AnyTelegramId(), "Alice", AnyDateOfBirth());

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.DomainEvents, Has.Some.InstanceOf<UserRegistered>());
        });
    }

    [Test]
    public void SetBodyMetrics_StoresMetrics()
    {
        var user = UserProfile.Register(AnyTelegramId(), "Alice", AnyDateOfBirth()).Value;
        var metrics = BodyMetrics.Create(heightCm: 180, weightKg: 75).Value;

        var result = user.SetBodyMetrics(metrics);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(user.BodyMetrics, Is.EqualTo(metrics));
            Assert.That(user.UpdatedAt, Is.Not.Null);
        });
    }
}
