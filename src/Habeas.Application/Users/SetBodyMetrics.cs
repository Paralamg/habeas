using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users;

public class SetBodyMetrics
{
    /// <summary>Records the latest height/weight for an already-registered user.</summary>
    public sealed record Command(long TelegramUserId, double HeightCm, double WeightKg)
        : ICommand<BodyMetricsView>;

    public sealed class Handler(IUserRepository users, IUnitOfWork unitOfWork)
        : ICommandHandler<Command, BodyMetricsView>
    {
        public async Task<Result<BodyMetricsView>> Handle(
            Command command, CancellationToken cancellationToken)
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
}
