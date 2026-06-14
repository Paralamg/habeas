using Telegram.Bot;
using Telegram.Bot.Types;

namespace Habeas.Bot.Telegram;

/// <summary>
/// Owns the bot's command menu — the list behind Telegram's blue "Menu" button. New users see a
/// minimal menu; once registered, a chat-scoped menu replaces it with the full command set.
/// Command names must be lowercase and carry no leading slash; Telegram adds it in the UI.
/// </summary>
internal sealed class BotMenu(ITelegramBotClient botClient)
{
    /// <summary>Shown by default to anyone who hasn't registered yet.</summary>
    private static readonly BotCommand[] NewUserCommands =
    [
        new() { Command = "start", Description = "Register and get started" },
        new() { Command = "help", Description = "Show the list of commands" },
    ];

    /// <summary>Shown to a registered user in their own chat, overriding the default menu.</summary>
    private static readonly BotCommand[] RegisteredUserCommands =
    [
        new() { Command = "birth", Description = "Set or change your date of birth" },
        new() { Command = "body", Description = "Record a body measurement (height or weight)" },
        new() { Command = "me", Description = "Show your profile and BMI" },
        new() { Command = "history", Description = "Show all recorded measurements over time" },
        new() { Command = "help", Description = "Show the list of commands" },
    ];

    /// <summary>
    /// Publishes the default (new-user) menu. Called once at startup; it applies to every chat
    /// that doesn't have a more specific, chat-scoped menu set.
    /// </summary>
    public Task PublishDefaultAsync(CancellationToken ct) =>
        botClient.SetMyCommands(NewUserCommands, cancellationToken: ct);

    /// <summary>
    /// Upgrades a single chat to the full registered-user menu. In private chats the chat id
    /// equals the Telegram user id. Telegram persists this until explicitly changed.
    /// </summary>
    public Task PublishForRegisteredAsync(long chatId, CancellationToken ct) =>
        botClient.SetMyCommands(
            RegisteredUserCommands, new BotCommandScopeChat { ChatId = chatId }, cancellationToken: ct);
}
