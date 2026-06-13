using System.Globalization;
using Habeas.Application.Common;
using Habeas.Application.Users;
using Habeas.Domain.Common;
using Habeas.Domain.Users;
using Telegram.Bot.Types.ReplyMarkups;

namespace Habeas.Bot.Telegram;

/// <summary>
/// Translates inbound text commands and button callbacks into application use cases and formats
/// the reply. This is the bot's "controller": it owns no business rules, only parsing and
/// presentation. Add a new command by adding a branch here and the matching use case in the
/// application layer.
/// </summary>
internal sealed class BotCommandRouter(
    ICommandHandler<RegisterUser.Command, Guid> registerUser,
    ICommandHandler<RecordBodyMeasurement.Command, MeasurementView> recordMeasurement,
    IQueryHandler<GetProfile.Query, GetProfile.ProfileView> getProfile,
    BodyConversationState conversations)
{
    /// <summary>Prefix for <c>/body</c> button callback data, e.g. <c>body:height</c>.</summary>
    private const string BodyCallbackPrefix = "body:";

    public async Task<BotResponse> RouteAsync(long telegramUserId, string displayName, string text, CancellationToken ct)
    {
        // A pending /body prompt takes precedence over free text. Typing another command instead
        // abandons the prompt (consumed here) and routes the command normally.
        if (conversations.TryConsume(telegramUserId, out var pending) && !text.TrimStart().StartsWith('/'))
        {
            return await HandleMeasurementValueAsync(telegramUserId, pending, text, ct);
        }

        var (command, argument) = Split(text);

        return command switch
        {
            "/start" => await HandleStartAsync(telegramUserId, displayName, argument, ct),
            "/body" => HandleBody(),
            "/me" => await HandleMeAsync(telegramUserId, ct),
            "/help" => HelpText,
            _ => $"Unknown command. {HelpText}",
        };
    }

    /// <summary>Handles a tap on a <c>/body</c> metric button, asking for the value next.</summary>
    public BotResponse HandleCallback(long telegramUserId, string callbackData)
    {
        if (!callbackData.StartsWith(BodyCallbackPrefix, StringComparison.Ordinal)
            || MetricType.FromKey(callbackData[BodyCallbackPrefix.Length..]) is not { } metricType)
        {
            return "Sorry, I didn't recognise that choice. Send /body to try again.";
        }

        conversations.Await(telegramUserId, metricType);
        return $"Enter your {metricType.DisplayName.ToLowerInvariant()} in {metricType.Unit} "
            + $"(field: {metricType.Key}):";
    }

    private async Task<string> HandleStartAsync(
        long telegramUserId, string displayName, string argument, CancellationToken ct)
    {
        // Date of birth is required at registration: it underpins age-aware body analysis.
        // Text format: "/start <YYYY-MM-DD>"  e.g.  /start 1990-05-20
        if (string.IsNullOrWhiteSpace(argument))
        {
            return "To register, send your date of birth: /start <YYYY-MM-DD>  e.g.  /start 1990-05-20";
        }

        if (!DateOnly.TryParseExact(
                argument.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
        {
            return "I couldn't read that date. Use the format YYYY-MM-DD, e.g. /start 1990-05-20";
        }

        var result = await registerUser.Handle(
            new RegisterUser.Command(telegramUserId, displayName, dateOfBirth), ct);
        return result.IsSuccess
            ? $"Welcome, {displayName}! You're registered. {HelpText}"
            : Fail(result.Error);
    }

    /// <summary>Presents the metric buttons; the actual value is captured in a follow-up message.</summary>
    private static BotResponse HandleBody()
    {
        var buttons = MetricType.All
            .Select(m => InlineKeyboardButton.WithCallbackData(m.DisplayName, $"{BodyCallbackPrefix}{m.Key}"));
        var keyboard = new InlineKeyboardMarkup(buttons);

        return new BotResponse("Which measurement would you like to record?", keyboard);
    }

    private async Task<string> HandleMeasurementValueAsync(
        long telegramUserId, MetricType metricType, string text, CancellationToken ct)
    {
        if (!double.TryParse(text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            // Keep the prompt active so the user can simply retry with a number.
            conversations.Await(telegramUserId, metricType);
            return $"That doesn't look like a number. Enter your {metricType.DisplayName.ToLowerInvariant()} "
                + $"in {metricType.Unit}, e.g. 180.";
        }

        var result = await recordMeasurement.Handle(
            new RecordBodyMeasurement.Command(telegramUserId, metricType.Key, value), ct);
        if (result.IsFailure)
        {
            return Fail(result.Error);
        }

        var saved = result.Value;
        return $"Saved: {saved.Metric.ToLowerInvariant()} {saved.Value:0.#} {saved.Unit}.";
    }

    private async Task<string> HandleMeAsync(long telegramUserId, CancellationToken ct)
    {
        var result = await getProfile.Handle(new GetProfile.Query(telegramUserId), ct);
        if (result.IsFailure)
        {
            return Fail(result.Error);
        }

        var profile = result.Value;
        var header = $"{profile.DisplayName}\n"
            + $"Born: {profile.DateOfBirth:yyyy-MM-dd} (age {profile.AgeYears})";

        if (profile.BodyMetrics is not { } m)
        {
            return $"{header}\nNo body metrics yet. Record them with /body.";
        }

        var lines = new List<string> { header };
        if (m.HeightCm is { } h)
        {
            lines.Add($"Height: {h:0.#} cm");
        }

        if (m.WeightKg is { } w)
        {
            lines.Add($"Weight: {w:0.#} kg");
        }

        if (m.Bmi is { } bmi)
        {
            lines.Add($"BMI: {bmi:0.0}");
        }

        return string.Join('\n', lines);
    }

    private static (string Command, string Argument) Split(string text)
    {
        var trimmed = text.Trim();
        var spaceIndex = trimmed.IndexOf(' ');
        return spaceIndex < 0
            ? (trimmed.ToLowerInvariant(), string.Empty)
            : (trimmed[..spaceIndex].ToLowerInvariant(), trimmed[(spaceIndex + 1)..].Trim());
    }

    private static string Fail(Error error) => $"⚠️ {error.Message}";

    private const string HelpText =
        "Commands:\n"
        + "/start <YYYY-MM-DD> — register with your date of birth\n"
        + "/body — record a body measurement (height or weight)\n"
        + "/me — show your profile and BMI";
}
