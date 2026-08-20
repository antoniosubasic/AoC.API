using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AoC.API;

/// <summary>
/// A validated Advent of Code year.
/// </summary>
/// <remarks>
/// The constructor is private and <see cref="Of(int)"/> is the only way in, so
/// a year that never had an event cannot become a request. Which days an event
/// has is <see cref="HasDay(Day)"/>, and the pairing of a year with a day is
/// checked by <see cref="Puzzle"/>.
/// </remarks>
public sealed record Year : IComparable<Year>
{
    /// <summary>The first year Advent of Code ran.</summary>
    public static Year First { get; } = new(2015);

    /// <summary>
    /// The first shortened event. From 2025 on, Advent of Code publishes 12
    /// puzzles instead of 25.
    /// </summary>
    public static Year FirstShort { get; } = new(2025);

    private Year(int value) => Value = value;

    /// <summary>The year itself.</summary>
    public int Value { get; }

    /// <summary>The last day this event publishes a puzzle on.</summary>
    public Day LastDay => Value >= FirstShort.Value ? Day.LastShort : Day.LastFull;

    /// <summary>Creates a year, rejecting anything before <see cref="First"/>.</summary>
    /// <param name="year">The year to validate.</param>
    /// <returns>The validated year.</returns>
    /// <exception cref="PuzzleException">The year never had an event.</exception>
    public static Year Of(int year) =>
        TryOf(year, out var validated) ? validated : throw PuzzleException.ForYear(year, nameof(year));

    /// <summary>Creates a year, reporting rather than throwing when there is none.</summary>
    /// <param name="year">The year to validate.</param>
    /// <param name="validated">The validated year, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="year"/> had an event.</returns>
    public static bool TryOf(int year, [NotNullWhen(true)] out Year? validated)
    {
        validated = year >= First.Value ? new Year(year) : null;
        return validated is not null;
    }

    /// <summary>Whether this event has a puzzle on the given day.</summary>
    /// <param name="day">The day to look for.</param>
    /// <returns><see langword="true"/> if the event published that day.</returns>
    public bool HasDay(Day day)
    {
        ArgumentNullException.ThrowIfNull(day);
        return day.Value <= LastDay.Value;
    }

    /// <inheritdoc />
    public int CompareTo(Year? other) => other is null ? 1 : Value.CompareTo(other.Value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <summary>Compares two years.</summary>
    /// <param name="left">The year on the left.</param>
    /// <param name="right">The year on the right.</param>
    /// <returns>Whether <paramref name="left"/> is earlier than <paramref name="right"/>.</returns>
    public static bool operator <(Year? left, Year? right) => Compare(left, right) < 0;

    /// <summary>Compares two years.</summary>
    /// <param name="left">The year on the left.</param>
    /// <param name="right">The year on the right.</param>
    /// <returns>Whether <paramref name="left"/> is not later than <paramref name="right"/>.</returns>
    public static bool operator <=(Year? left, Year? right) => Compare(left, right) <= 0;

    /// <summary>Compares two years.</summary>
    /// <param name="left">The year on the left.</param>
    /// <param name="right">The year on the right.</param>
    /// <returns>Whether <paramref name="left"/> is later than <paramref name="right"/>.</returns>
    public static bool operator >(Year? left, Year? right) => Compare(left, right) > 0;

    /// <summary>Compares two years.</summary>
    /// <param name="left">The year on the left.</param>
    /// <param name="right">The year on the right.</param>
    /// <returns>Whether <paramref name="left"/> is not earlier than <paramref name="right"/>.</returns>
    public static bool operator >=(Year? left, Year? right) => Compare(left, right) >= 0;

    private static int Compare(Year? left, Year? right) =>
        left is null ? (right is null ? 0 : -1) : left.CompareTo(right);
}
