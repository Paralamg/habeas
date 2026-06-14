using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users;

public class RegisterUser
{
    /// <summary>Registers a Telegram user the first time they interact with the bot.</summary>
    public sealed record Command(long TelegramUserId, string DisplayName) : ICommand<Registration>;

    /// <summary>Outcome of registration: the user's id and whether they were newly created.</summary>
    public sealed record Registration(Guid UserId, bool WasCreated);

    public sealed class Handler(IUserRepository users, IUnitOfWork unitOfWork)
        : ICommandHandler<Command, Registration>
    {
        public async Task<Result<Registration>> Handle(Command command, CancellationToken cancellationToken)
        {
            var existing = await users.GetByTelegramIdAsync(command.TelegramUserId, cancellationToken);
            if (existing is not null)
            {
                // Registration is idempotent: re-issuing /start must not create a duplicate.
                return new Registration(existing.Id.Value, WasCreated: false);
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

            return new Registration(profile.Value.Id.Value, WasCreated: true);
        }
    }
}
