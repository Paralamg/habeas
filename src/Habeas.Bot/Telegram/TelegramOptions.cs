using System.ComponentModel.DataAnnotations;

namespace Habeas.Bot.Telegram;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    /// <summary>Bot token issued by @BotFather.</summary>
    [Required]
    public string BotToken { get; init; } = string.Empty;
}
