using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users.SetBodyMetrics;

public sealed class SetBodyMetricsCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<SetBodyMetricsCommand, BodyMetricsView>
{
    public async Task<Result<BodyMetricsView>> Handle(
        SetBodyMetricsCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByTelegramIdAsync(command.TelegramUserId, cancellationToken);
        if (user is null)
        {
            return Error.NotFound("You are not registered yet. Send /start first.");
        }

        var metrics = BodyMetrics.Create(command.HeightCm, command.WeightKg);
        if (metrics.IsFailure)
        {
            return metrics.Error;
        }

        user.SetBodyMetrics(metrics.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = metrics.Value;
        return new BodyMetricsView(saved.HeightCm, saved.WeightKg, saved.Bmi);
    }
}
