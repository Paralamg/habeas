using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users;

public class RecordBodyMeasurement
{
    /// <summary>Appends a single body measurement (e.g. height or weight) for a registered user.</summary>
    public sealed record Command(long TelegramUserId, string MetricKey, double Value)
        : ICommand<MeasurementView>;

    public sealed class Handler(IUserRepository users, IUnitOfWork unitOfWork, IClock clock)
        : ICommandHandler<Command, MeasurementView>
    {
        public async Task<Result<MeasurementView>> Handle(
            Command command, CancellationToken cancellationToken)
        {
            var user = await users.GetByTelegramIdAsync(command.TelegramUserId, cancellationToken);
            if (user is null)
            {
                return Error.NotFound("You are not registered yet. Send /start first.");
            }

            var metricType = MetricType.FromKey(command.MetricKey);
            if (metricType is null)
            {
                return Error.Validation($"Unknown metric '{command.MetricKey}'.");
            }

            var recorded = clock.UtcNow;
            var result = user.RecordMeasurement(metricType, command.Value, recorded);
            if (result.IsFailure)
            {
                return result.Error;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new MeasurementView(metricType.DisplayName, command.Value, metricType.Unit, recorded);
        }
    }
}
