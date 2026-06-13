using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Habeas.Bot.Telegram;

/// <summary>
/// Receives raw Telegram updates, opens a DI scope per update, and delegates command
/// handling to <see cref="BotCommandRouter"/>. Keeps transport concerns out of the
/// application layer.
/// </summary>
internal sealed class HabeasUpdateHandler(IServiceScopeFactory scopeFactory, ILogger<HabeasUpdateHandler> logger)
    : IUpdateHandler
{
    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Text: { Length: > 0 } text } message)
        {
            return;
        }

        var from = message.From;
        if (from is null || from.IsBot)
        {
            return;
        }

        var displayName = string.IsNullOrWhiteSpace(from.Username)
            ? $"{from.FirstName} {from.LastName}".Trim()
            : from.Username!;

        string reply;
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var router = scope.ServiceProvider.GetRequiredService<BotCommandRouter>();
            reply = await router.RouteAsync(from.Id, displayName, text, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle update from {UserId}.", from.Id);
            reply = "Something went wrong. Please try again later.";
        }

        await botClient.SendMessage(message.Chat.Id, reply, cancellationToken: cancellationToken);
    }

    public Task HandleErrorAsync(
        ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Telegram polling error from {Source}.", source);
        return Task.CompletedTask;
    }
}
