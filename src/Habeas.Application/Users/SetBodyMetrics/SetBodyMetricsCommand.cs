using Habeas.Application.Common;

namespace Habeas.Application.Users.SetBodyMetrics;

/// <summary>Records the latest height/weight for an already-registered user.</summary>
public sealed record SetBodyMetricsCommand(long TelegramUserId, double HeightCm, double WeightKg)
    : ICommand<BodyMetricsView>;
