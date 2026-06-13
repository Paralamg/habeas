using Habeas.Application.Common;

namespace Habeas.Application.Users.GetProfile;

/// <summary>Reads a user's profile, including their latest body metrics if recorded.</summary>
public sealed record GetProfileQuery(long TelegramUserId) : IQuery<ProfileView>;

/// <summary>Read model returned to the presentation layer.</summary>
public sealed record ProfileView(string DisplayName, BodyMetricsView? BodyMetrics);
