using Habeas.Application.Common;
using Habeas.Domain.Common;
using Habeas.Domain.Users;

namespace Habeas.Application.Users;

public class SetDateOfBirth
{
    /// <summary>Sets or changes the date of birth for an already-registered user.</summary>
    public sealed record Command(long TelegramUserId, DateOnly DateOfBirth) : ICommand<DateOfBirthView>;

    /// <summary>Read model echoing the stored date of birth and the resulting age.</summary>
    public sealed record DateOfBirthView(DateOnly DateOfBirth, int AgeYears);

    public sealed class Handler(IUserRepository users, IUnitOfWork unitOfWork)
        : ICommandHandler<Command, DateOfBirthView>
    {
        public async Task<Result<DateOfBirthView>> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await users.GetByTelegramIdAsync(command.TelegramUserId, cancellationToken);
            if (user is null)
            {
                return Error.NotFound("You are not registered yet. Send /start first.");
            }

            var dateOfBirth = DateOfBirth.Create(command.DateOfBirth);
            if (dateOfBirth.IsFailure)
            {
                return dateOfBirth.Error;
            }

            user.SetDateOfBirth(dateOfBirth.Value);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return new DateOfBirthView(dateOfBirth.Value.Value, dateOfBirth.Value.AgeInYears(today));
        }
    }
}
