using Telegram.Bot.Types.ReplyMarkups;

namespace Habeas.Bot.Telegram;

/// <summary>
/// A reply the router wants the transport layer to send: text plus an optional inline keyboard.
/// Plain text replies convert implicitly, so most call sites can keep returning strings.
/// </summary>
internal sealed record BotResponse(string Text, InlineKeyboardMarkup? ReplyMarkup = null)
{
    public static implicit operator BotResponse(string text) => new(text);
}
