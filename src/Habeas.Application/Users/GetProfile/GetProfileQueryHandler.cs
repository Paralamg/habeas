using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users.GetProfile;

public sealed class GetProfileQueryHandler(IUserRepository users)
    : IQueryHandler<GetProfileQuery, ProfileView>
{
    public async Task<Result<ProfileView>> Handle(GetProfileQuery query, CancellationToken cancellationToken)
    {
        var user = await users.GetByTelegramIdAsync(query.TelegramUserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("You are not registered yet. Send /start first.");
        }

        var metrics = user.BodyMetrics is { } m
            ? new BodyMetricsView(m.HeightCm, m.WeightKg, m.Bmi)
            : null;

        return new ProfileView(user.DisplayName, metrics);
    }
}
