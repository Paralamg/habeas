using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Habeas.Bot.Telegram;

/// <summary>
/// Long-running hosted service that drives Telegram long-polling for the lifetime of the
/// application, dispatching every update to <see cref="HabeasUpdateHandler"/>.
/// </summary>
internal sealed class BotPollingService(
    ITelegramBotClient botClient,
    HabeasUpdateHandler updateHandler,
    BotMenu menu,
    ILogger<BotPollingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Publishing commands is what makes Telegram render the Menu button; do it before polling.
        await menu.PublishDefaultAsync(stoppingToken);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
            DropPendingUpdates = true,
        };

        logger.LogInformation("Habeas bot started; listening for updates.");
        await botClient.ReceiveAsync(updateHandler, receiverOptions, stoppingToken);
    }
}
