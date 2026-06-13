using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users;

public class GetMeasurementHistory
{
    /// <summary>Reads the full time series of a user's recorded body measurements.</summary>
    public sealed record Query(long TelegramUserId) : IQuery<HistoryView>;

    /// <summary>All measurements, oldest first, so callers can show how each metric changed.</summary>
    public sealed record HistoryView(IReadOnlyList<MeasurementView> Measurements);

    public sealed class Handler(IUserRepository users) : IQueryHandler<Query, HistoryView>
    {
        public async Task<Result<HistoryView>> Handle(Query query, CancellationToken cancellationToken)
        {
            var user = await users.GetByTelegramIdAsync(query.TelegramUserId, cancellationToken);
            if (user is null)
            {
                return Error.NotFound("You are not registered yet. Send /start first.");
            }

            var measurements = user.Measurements
                .OrderBy(m => m.RecordedAt)
                .Select(m => new MeasurementView(m.MetricType.DisplayName, m.Value, m.MetricType.Unit, m.RecordedAt))
                .ToList();

            return new HistoryView(measurements);
        }
    }
}
