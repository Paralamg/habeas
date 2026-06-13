using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users;

public class GetProfile
{
    /// <summary>Reads a user's profile, including their latest body metrics if recorded.</summary>
    public sealed record Query(long TelegramUserId) : IQuery<ProfileView>;

    /// <summary>Read model returned to the presentation layer.</summary>
    public sealed record ProfileView(
        string DisplayName, DateOnly? DateOfBirth, int? AgeYears, BodyMetricsView? BodyMetrics);

    public sealed class Handler(IUserRepository users) : IQueryHandler<Query, ProfileView>
    {
        public async Task<Result<ProfileView>> Handle(Query query, CancellationToken cancellationToken)
        {
            var user = await users.GetByTelegramIdAsync(query.TelegramUserId, cancellationToken);
            if (user is null)
            {
                return Error.NotFound("You are not registered yet. Send /start first.");
            }

            var height = user.LatestOf(MetricType.Height);
            var weight = user.LatestOf(MetricType.Weight);
            var metrics = height is null && weight is null
                ? null
                : new BodyMetricsView(height?.Value, weight?.Value, user.CurrentBmi);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return new ProfileView(
                user.DisplayName,
                user.DateOfBirth?.Value,
                user.DateOfBirth?.AgeInYears(today),
                metrics);
        }
    }
}