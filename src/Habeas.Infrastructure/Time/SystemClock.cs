using Habeas.Application.Common;

namespace Habeas.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
