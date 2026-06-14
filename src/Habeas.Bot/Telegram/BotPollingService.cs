using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Habeas.Bot.Telegram;

/// <summary>
/// Long-running hosted service that drives Telegram long-polling for the lifetime of the
/// application, dispatching every update to <see cref="HabeasUpdateHandler"/>.
/// </summary>
internal sealed class BotPollingService(
    ITelegramBotClient botClient,
    HabeasUpdateHandler updateHandler,
    ILogger<BotPollingService> logger) : BackgroundService
{
    /// <summary>
    /// Commands published to Telegram so the blue "Menu" button next to the input field lists
    /// them. Names must be lowercase and carry no leading slash; Telegram adds it in the UI.
    /// </summary>
    private static readonly BotCommand[] MenuCommands =
    [
        new() { Command = "start", Description = "Register and get started" },
        new() { Command = "birth", Description = "Set or change your date of birth" },
        new() { Command = "body", Description = "Record a body measurement (height or weight)" },
        new() { Command = "me", Description = "Show your profile and BMI" },
        new() { Command = "history", Description = "Show all recorded measurements over time" },
        new() { Command = "help", Description = "Show the list of commands" },
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Publishing commands is what makes Telegram render the Menu button; do it before polling.
        await botClient.SetMyCommands(MenuCommands, cancellationToken: stoppingToken);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
            DropPendingUpdates = true,
        };

        logger.LogInformation("Habeas bot started; listening for updates.");
        await botClient.ReceiveAsync(updateHandler, receiverOptions, stoppingToken);
    }
}
