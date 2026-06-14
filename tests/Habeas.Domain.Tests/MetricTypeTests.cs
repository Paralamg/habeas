using Habeas.Domain.Users;

namespace Habeas.Domain.Tests;

[TestFixture]
public sealed class MetricTypeTests
{
    [Test]
    public void FromKey_KnownKey_ReturnsMetric_CaseInsensitive()
    {
        Assert.Multiple(() =>
        {
            Assert.That(MetricType.FromKey("height"), Is.EqualTo(MetricType.Height));
            Assert.That(MetricType.FromKey("WEIGHT"), Is.EqualTo(MetricType.Weight));
        });
    }

    [Test]
    public void FromKey_UnknownKey_ReturnsNull()
    {
        Assert.That(MetricType.FromKey("bodyfat"), Is.Null);
    }

    [Test]
    public void Validate_RejectsValuesOutsideRange()
    {
        Assert.Multiple(() =>
        {
            Assert.That(MetricType.Height.Validate(0).IsFailure, Is.True);
            Assert.That(MetricType.Height.Validate(301).IsFailure, Is.True);
            Assert.That(MetricType.Height.Validate(180).IsSuccess, Is.True);
        });
    }
}
