using System.Globalization;
using Habeas.Application.Common;
using Habeas.Application.Users;
using Habeas.Domain.Common;

namespace Habeas.Bot.Telegram;

/// <summary>
/// Translates inbound text commands into application use cases and formats the reply.
/// This is the bot's "controller": it owns no business rules, only parsing and presentation.
/// Add a new command by adding a branch here and the matching use case in the application layer.
/// </summary>
internal sealed class BotCommandRouter(
    ICommandHandler<RegisterUser.Command, Guid> registerUser,
    ICommandHandler<SetBodyMetrics.Command, BodyMetricsView> setBodyMetrics,
    IQueryHandler<GetProfile.Query, GetProfile.ProfileView> getProfile)
{
    public async Task<string> RouteAsync(long telegramUserId, string displayName, string text, CancellationToken ct)
    {
        var (command, argument) = Split(text);

        return command switch
        {
            "/start" => await HandleStartAsync(telegramUserId, displayName, argument, ct),
            "/body" => await HandleBodyAsync(telegramUserId, argument, ct),
            "/me" => await HandleMeAsync(telegramUserId, ct),
            "/help" => HelpText,
            _ => $"Unknown command. {HelpText}",
        };
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

    private async Task<string> HandleBodyAsync(long telegramUserId, string argument, CancellationToken ct)
    {
        // Text format: "/body <height_cm> <weight_kg>"  e.g.  /body 180 75
        var parts = argument.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var heightCm)
            || !double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var weightKg))
        {
            return "Usage: /body <height_cm> <weight_kg>  e.g.  /body 180 75";
        }

        var command = new SetBodyMetrics.Command(telegramUserId, heightCm, weightKg);
        var result = await setBodyMetrics.Handle(command, ct);
        return result.IsSuccess
            ? $"Saved: {result.Value.HeightCm:0.#} cm, {result.Value.WeightKg:0.#} kg. BMI: {result.Value.Bmi:0.0}."
            : Fail(result.Error);
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
            return $"{header}\nNo body metrics yet. Record them with /body <height_cm> <weight_kg>.";
        }

        return $"{header}\nHeight: {m.HeightCm:0.#} cm\nWeight: {m.WeightKg:0.#} kg\nBMI: {m.Bmi:0.0}";
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
        + "/body <height_cm> <weight_kg> — record your height and weight\n"
        + "/me — show your profile and BMI";
}
