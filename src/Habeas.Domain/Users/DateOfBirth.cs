using Habeas.Domain.Common;

namespace Habeas.Domain.Users;

/// <summary>
/// The user's date of birth — a core fact for the bot, since body-metric analysis (e.g.
/// age-adjusted recommendations) depends on it. Stored as a calendar date with no time
/// component; validated to be a plausible birth date for a living person.
/// </summary>
public sealed class DateOfBirth : ValueObject
{
    /// <summary>Oldest plausible age, used as a sanity bound on the birth date.</summary>
    private const int MaxAgeYears = 120;

    private DateOfBirth(DateOnly value) => Value = value;

    public DateOnly Value { get; }

    public static Result<DateOfBirth> Create(DateOnly value)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (value > today)
        {
            return Error.Validation("Date of birth cannot be in the future.");
        }

        if (value < today.AddYears(-MaxAgeYears))
        {
            return Error.Validation($"Date of birth cannot be more than {MaxAgeYears} years ago.");
        }

        return new DateOfBirth(value);
    }

    /// <summary>
    /// Reconstructs the value object from already-validated persisted data. For use by the
    /// persistence layer only — application code must go through <see cref="Create"/>.
    /// </summary>
    public static DateOfBirth FromTrusted(DateOnly value) => new(value);

    /// <summary>Age in completed years as of <paramref name="asOf"/>.</summary>
    public int AgeInYears(DateOnly asOf)
    {
        var age = asOf.Year - Value.Year;
        if (Value > asOf.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
