using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AoC.API;

/// <summary>
/// A validated Advent of Code day, always within <c>1..=25</c>.
/// </summary>
/// <remarks>
/// The upper bound is the widest range any event has had; which days a
/// <em>particular</em> event has is <see cref="Year.HasDay(Day)"/>, enforced by
/// <see cref="Puzzle.Of(Year, Day)"/>.
/// </remarks>
public sealed record Day : IComparable<Day>
{
    /// <summary>The first puzzle day.</summary>
    public static Day First { get; } = new(1);

    /// <summary>The last puzzle day of events up to and including 2024.</summary>
    public static Day LastFull { get; } = new(25);

    /// <summary>The last puzzle day from <see cref="Year.FirstShort"/> on.</summary>
    public static Day LastShort { get; } = new(12);

    private Day(int value) => Value = value;

    /// <summary>The day itself.</summary>
    public int Value { get; }

    /// <summary>Creates a day, rejecting anything outside <c>1..=25</c>.</summary>
    /// <param name="day">The day to validate.</param>
    /// <returns>The validated day.</returns>
    /// <exception cref="PuzzleException">No event has ever published that day.</exception>
    public static Day Of(int day) =>
        TryOf(day, out var validated) ? validated : throw PuzzleException.ForDay(day, nameof(day));

    /// <summary>Creates a day, reporting rather than throwing when there is none.</summary>
    /// <param name="day">The day to validate.</param>
    /// <param name="validated">The validated day, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if some event has published that day.</returns>
    public static bool TryOf(int day, [NotNullWhen(true)] out Day? validated)
    {
        validated = day >= First.Value && day <= LastFull.Value ? new Day(day) : null;
        return validated is not null;
    }

    /// <inheritdoc />
    public int CompareTo(Day? other) => other is null ? 1 : Value.CompareTo(other.Value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <summary>Compares two days.</summary>
    /// <param name="left">The day on the left.</param>
    /// <param name="right">The day on the right.</param>
    /// <returns>Whether <paramref name="left"/> is earlier than <paramref name="right"/>.</returns>
    public static bool operator <(Day? left, Day? right) => Compare(left, right) < 0;

    /// <summary>Compares two days.</summary>
    /// <param name="left">The day on the left.</param>
    /// <param name="right">The day on the right.</param>
    /// <returns>Whether <paramref name="left"/> is not later than <paramref name="right"/>.</returns>
    public static bool operator <=(Day? left, Day? right) => Compare(left, right) <= 0;

    /// <summary>Compares two days.</summary>
    /// <param name="left">The day on the left.</param>
    /// <param name="right">The day on the right.</param>
    /// <returns>Whether <paramref name="left"/> is later than <paramref name="right"/>.</returns>
    public static bool operator >(Day? left, Day? right) => Compare(left, right) > 0;

    /// <summary>Compares two days.</summary>
    /// <param name="left">The day on the left.</param>
    /// <param name="right">The day on the right.</param>
    /// <returns>Whether <paramref name="left"/> is not earlier than <paramref name="right"/>.</returns>
    public static bool operator >=(Day? left, Day? right) => Compare(left, right) >= 0;

    private static int Compare(Day? left, Day? right) =>
        left is null ? (right is null ? 0 : -1) : left.CompareTo(right);
}
