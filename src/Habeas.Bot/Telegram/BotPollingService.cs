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
    ILogger<BotPollingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message],
            DropPendingUpdates = true,
        };

        logger.LogInformation("Habeas bot started; listening for updates.");
        await botClient.ReceiveAsync(updateHandler, receiverOptions, stoppingToken);
    }
}
