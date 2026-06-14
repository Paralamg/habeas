using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Habeas.Bot.Telegram;

/// <summary>
/// Receives raw Telegram updates, opens a DI scope per update, and delegates command and
/// callback handling to <see cref="BotCommandRouter"/>. Keeps transport concerns out of the
/// application layer.
/// </summary>
internal sealed class HabeasUpdateHandler(IServiceScopeFactory scopeFactory, ILogger<HabeasUpdateHandler> logger)
    : IUpdateHandler
{
    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        switch (update)
        {
            case { Message: { Text: { Length: > 0 } } message }:
                await HandleMessageAsync(botClient, message, cancellationToken);
                break;
            case { CallbackQuery: { } callbackQuery }:
                await HandleCallbackAsync(botClient, callbackQuery, cancellationToken);
                break;
        }
    }

    private async Task HandleMessageAsync(
        ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var from = message.From;
        if (from is null || from.IsBot)
        {
            return;
        }

        var displayName = DisplayNameOf(from);

        BotResponse response;
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var router = scope.ServiceProvider.GetRequiredService<BotCommandRouter>();
            response = await router.RouteAsync(from.Id, displayName, message.Text!, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle update from {UserId}.", from.Id);
            response = "Something went wrong. Please try again later.";
        }

        await botClient.SendMessage(
            message.Chat.Id, response.Text, replyMarkup: response.ReplyMarkup, cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackAsync(
        ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        // Always acknowledge so Telegram stops showing the button's loading state.
        await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

        if (callbackQuery is not { From: { IsBot: false } from, Message.Chat.Id: var chatId, Data: { } data })
        {
            return;
        }

        BotResponse response;
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var router = scope.ServiceProvider.GetRequiredService<BotCommandRouter>();
            response = router.HandleCallback(from.Id, data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle callback from {UserId}.", from.Id);
            response = "Something went wrong. Please try again later.";
        }

        await botClient.SendMessage(
            chatId, response.Text, replyMarkup: response.ReplyMarkup, cancellationToken: cancellationToken);
    }

    private static string DisplayNameOf(User from) =>
        string.IsNullOrWhiteSpace(from.Username)
            ? $"{from.FirstName} {from.LastName}".Trim()
            : from.Username!;

    public Task HandleErrorAsync(
        ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Telegram polling error from {Source}.", source);
        return Task.CompletedTask;
    }
}
