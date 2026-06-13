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
    public void RecordMeasurement_OutOfRange_Fails()
    {
        var user = UserProfile.Register(AnyTelegramId(), "Alice", AnyDateOfBirth()).Value;

        var result = user.RecordMeasurement(MetricType.Height, value: 500, At(2026, 6, 13));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(user.Measurements, Is.Empty);
        });
    }

    [Test]
    public void RecordMeasurement_StoresMeasurementAndTouches()
    {
        var user = UserProfile.Register(AnyTelegramId(), "Alice", AnyDateOfBirth()).Value;

        var result = user.RecordMeasurement(MetricType.Weight, value: 75, At(2026, 6, 13));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(user.Measurements, Has.Count.EqualTo(1));
            Assert.That(user.UpdatedAt, Is.Not.Null);
        });
    }

    [Test]
    public void LatestOf_ReturnsMostRecentMeasurementOfType()
    {
        var user = UserProfile.Register(AnyTelegramId(), "Alice", AnyDateOfBirth()).Value;
        user.RecordMeasurement(MetricType.Weight, value: 75, At(2026, 6, 1));
        user.RecordMeasurement(MetricType.Weight, value: 74, At(2026, 6, 13));

        Assert.That(user.LatestOf(MetricType.Weight)!.Value, Is.EqualTo(74));
    }

    [Test]
    public void CurrentBmi_IsNullUntilBothHeightAndWeightRecorded()
    {
        var user = UserProfile.Register(AnyTelegramId(), "Alice", AnyDateOfBirth()).Value;
        user.RecordMeasurement(MetricType.Height, value: 180, At(2026, 6, 1));

        Assert.That(user.CurrentBmi, Is.Null);

        user.RecordMeasurement(MetricType.Weight, value: 75, At(2026, 6, 2));

        Assert.That(user.CurrentBmi, Is.EqualTo(75d / Math.Pow(1.8, 2)).Within(0.0001));
    }

    private static DateTimeOffset At(int year, int month, int day) =>
        new(new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc));
}
