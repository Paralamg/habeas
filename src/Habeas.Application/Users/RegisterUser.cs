using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users;

public class RegisterUser
{
    /// <summary>Registers a Telegram user the first time they interact with the bot.</summary>
    public sealed record Command(long TelegramUserId, string DisplayName) : ICommand<Guid>;

    public sealed class Handler(IUserRepository users, IUnitOfWork unitOfWork)
        : ICommandHandler<Command, Guid>
    {
        public async Task<Result<Guid>> Handle(Command command, CancellationToken cancellationToken)
        {
            var existing = await users.GetByTelegramIdAsync(command.TelegramUserId, cancellationToken);
            if (existing is not null)
            {
                // Registration is idempotent: re-issuing /start must not create a duplicate.
                return existing.Id.Value;
            }

            var telegramId = TelegramUserId.Create(command.TelegramUserId);
            if (telegramId.IsFailure)
            {
                return telegramId.Error;
            }

            var profile = UserProfile.Register(telegramId.Value, command.DisplayName);
            if (profile.IsFailure)
            {
                return profile.Error;
            }

            await users.AddAsync(profile.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return profile.Value.Id.Value;
        }
    }
}
