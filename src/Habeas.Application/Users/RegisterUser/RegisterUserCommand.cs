using Habeas.Application.Common;

namespace Habeas.Application.Users.RegisterUser;

/// <summary>Registers a Telegram user the first time they interact with the bot.</summary>
public sealed record RegisterUserCommand(long TelegramUserId, string DisplayName) : ICommand<Guid>;
