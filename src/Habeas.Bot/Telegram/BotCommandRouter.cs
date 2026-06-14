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
    ICommandHandler<RegisterUser.Command, RegisterUser.Registration> registerUser,
    ICommandHandler<SetDateOfBirth.Command, SetDateOfBirth.DateOfBirthView> setDateOfBirth,
    ICommandHandler<RecordBodyMeasurement.Command, MeasurementView> recordMeasurement,
    IQueryHandler<GetProfile.Query, GetProfile.ProfileView> getProfile,
    IQueryHandler<GetMeasurementHistory.Query, GetMeasurementHistory.HistoryView> getHistory,
    ConversationState conversations,
    BotMenu menu)
{
    /// <summary>Prefix for <c>/body</c> button callback data, e.g. <c>body:height</c>.</summary>
    private const string BodyCallbackPrefix = "body:";

    public async Task<BotResponse> RouteAsync(long telegramUserId, string displayName, string text, CancellationToken ct)
    {
        // A pending prompt takes precedence over free text. Typing another command instead
        // abandons the prompt (consumed here) and routes the command normally.
        if (conversations.TryConsume(telegramUserId, out var pending) && !text.TrimStart().StartsWith('/'))
        {
            return pending switch
            {
                AwaitingMeasurement m => await HandleMeasurementValueAsync(telegramUserId, m.MetricType, text, ct),
                AwaitingDateOfBirth => await HandleDateOfBirthAsync(telegramUserId, text, ct),
                _ => HelpText,
            };
        }

        var (command, _) = Split(text);

        return command switch
        {
            "/start" => await HandleStartAsync(telegramUserId, displayName, ct),
            "/birth" => HandleBirth(telegramUserId),
            "/body" => HandleBody(),
            "/me" => await HandleMeAsync(telegramUserId, ct),
            "/history" => await HandleHistoryAsync(telegramUserId, ct),
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

        conversations.Await(telegramUserId, new AwaitingMeasurement(metricType));
        return $"Enter your {metricType.DisplayName.ToLowerInvariant()} in {metricType.Unit} ";
    }

    /// <summary>
    /// Registers the user if needed. New users are walked straight into the /birth flow; returning
    /// users are simply greeted and shown the command list.
    /// </summary>
    private async Task<BotResponse> HandleStartAsync(long telegramUserId, string displayName, CancellationToken ct)
    {
        var result = await registerUser.Handle(new RegisterUser.Command(telegramUserId, displayName), ct);
        if (result.IsFailure)
        {
            return Fail(result.Error);
        }

        // Now that the user is registered, swap their chat over to the full command menu.
        await menu.PublishForRegisteredAsync(telegramUserId, ct);

        if (result.Value.WasCreated)
        {
            return HandleBirth(telegramUserId, $"Welcome, {displayName}! Let's set your date of birth.\n");
        }

        return $"Welcome back, {displayName}!\n{HelpText}";
    }

    /// <summary>Prompts the user to type a date of birth; captured in a follow-up message.</summary>
    private string HandleBirth(long telegramUserId, string prefix = "")
    {
        conversations.Await(telegramUserId, new AwaitingDateOfBirth());
        return $"{prefix}Please enter your date of birth (YYYY-MM-DD), e.g. 1990-05-20";
    }

    private async Task<string> HandleDateOfBirthAsync(long telegramUserId, string text, CancellationToken ct)
    {
        if (!DateOnly.TryParseExact(
                text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
        {
            // Keep the prompt active so the user can simply retry with a valid date.
            conversations.Await(telegramUserId, new AwaitingDateOfBirth());
            return "I couldn't read that date. Use the format YYYY-MM-DD, e.g. 1990-05-20";
        }

        var result = await setDateOfBirth.Handle(
            new SetDateOfBirth.Command(telegramUserId, dateOfBirth), ct);
        if (result.IsFailure)
        {
            return Fail(result.Error);
        }

        return $"Saved your date of birth: {result.Value.DateOfBirth:yyyy-MM-dd} "
            + $"(age {result.Value.AgeYears}).\n{HelpText}";
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
            conversations.Await(telegramUserId, new AwaitingMeasurement(metricType));
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
        var lines = new List<string> { profile.DisplayName };

        lines.Add(profile.DateOfBirth is { } dob
            ? $"Born: {dob:yyyy-MM-dd} (age {profile.AgeYears})"
            : "Date of birth not set. Use /birth.");

        if (profile.BodyMetrics is not { } m)
        {
            lines.Add("No body metrics yet. Record them with /body.");
            return string.Join('\n', lines);
        }

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

    private async Task<string> HandleHistoryAsync(long telegramUserId, CancellationToken ct)
    {
        var result = await getHistory.Handle(new GetMeasurementHistory.Query(telegramUserId), ct);
        if (result.IsFailure)
        {
            return Fail(result.Error);
        }

        if (result.Value.Measurements.Count == 0)
        {
            return "No measurements yet. Record some with /body.";
        }

        // Group by metric so each characteristic's trend reads top-to-bottom (oldest first).
        var sections = result.Value.Measurements
            .GroupBy(m => (m.Metric, m.Unit))
            .Select(group =>
            {
                var rows = group.Select(m => $"  {m.RecordedAt:yyyy-MM-dd}: {m.Value:0.#} {m.Unit}");
                return $"{group.Key.Metric}:\n{string.Join('\n', rows)}";
            });

        return $"Measurement history:\n\n{string.Join("\n\n", sections)}";
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
        + "/start — register and get started\n"
        + "/birth — set or change your date of birth\n"
        + "/body — record a body measurement (height or weight)\n"
        + "/me — show your profile and BMI\n"
        + "/history — show all recorded measurements over time";
}
