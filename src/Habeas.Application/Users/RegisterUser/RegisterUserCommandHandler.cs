using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users.RegisterUser;

public sealed class RegisterUserCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
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
