using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AoC.API;

/// <summary>
/// A specific puzzle: one day of one event.
/// </summary>
/// <remarks>
/// Both coordinates are validated on their own, and so is the pairing: from
/// <see cref="Year.FirstShort"/> on, an event stops after
/// <see cref="Day.LastShort"/>, so day 25 of 2025 is not a puzzle even though
/// both halves are individually plausible. A puzzle also owns the three URLs
/// this library talks to, so nothing else has to build one.
/// </remarks>
public sealed record Puzzle : IComparable<Puzzle>
{
    /// <summary>The site every request in this library is made against.</summary>
    public const string BaseUrl = "https://adventofcode.com";

    private Puzzle(Year year, Day day)
    {
        Year = year;
        Day = day;
    }

    /// <summary>The event this puzzle belongs to.</summary>
    public Year Year { get; }

    /// <summary>The day within the event.</summary>
    public Day Day { get; }

    /// <summary>The canonical puzzle URL.</summary>
    public string Url => string.Create(CultureInfo.InvariantCulture, $"{BaseUrl}/{Year}/day/{Day}");

    /// <summary>The URL the puzzle's personal input is downloaded from.</summary>
    public string InputUrl => $"{Url}/input";

    /// <summary>The URL answers are submitted to.</summary>
    public string AnswerUrl => $"{Url}/answer";

    /// <summary>Creates a puzzle, rejecting a day the event never published.</summary>
    /// <param name="year">The event.</param>
    /// <param name="day">The day within it.</param>
    /// <returns>The puzzle.</returns>
    /// <exception cref="PuzzleException">That event has no such day.</exception>
    public static Puzzle Of(Year year, Day day)
    {
        ArgumentNullException.ThrowIfNull(year);
        ArgumentNullException.ThrowIfNull(day);

        return year.HasDay(day) ? new Puzzle(year, day) : throw PuzzleException.ForPairing(year, day, nameof(day));
    }

    /// <summary>Creates a puzzle from raw numbers, validating both and their pairing.</summary>
    /// <param name="year">The event year.</param>
    /// <param name="day">The day within it.</param>
    /// <returns>The puzzle.</returns>
    /// <exception cref="PuzzleException">Whichever coordinate is wrong, or their pairing.</exception>
    /// <example>
    /// <code>
    /// var puzzle = Puzzle.At(2024, 7);
    /// </code>
    /// </example>
    public static Puzzle At(int year, int day) => Of(Year.Of(year), Day.Of(day));

    /// <summary>Creates a puzzle, reporting rather than throwing when there is none.</summary>
    /// <param name="year">The event year.</param>
    /// <param name="day">The day within it.</param>
    /// <param name="puzzle">The puzzle, or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the coordinates name a puzzle.</returns>
    public static bool TryAt(int year, int day, [NotNullWhen(true)] out Puzzle? puzzle)
    {
        puzzle = Year.TryOf(year, out var validYear) && Day.TryOf(day, out var validDay) && validYear.HasDay(validDay)
            ? new Puzzle(validYear, validDay)
            : null;

        return puzzle is not null;
    }

    /// <inheritdoc />
    public int CompareTo(Puzzle? other)
    {
        if (other is null) { return 1; }

        var year = Year.CompareTo(other.Year);
        return year != 0 ? year : Day.CompareTo(other.Day);
    }

    /// <inheritdoc />
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Year} day {Day}");

    /// <summary>Compares two puzzles.</summary>
    /// <param name="left">The puzzle on the left.</param>
    /// <param name="right">The puzzle on the right.</param>
    /// <returns>Whether <paramref name="left"/> comes before <paramref name="right"/>.</returns>
    public static bool operator <(Puzzle? left, Puzzle? right) => Compare(left, right) < 0;

    /// <summary>Compares two puzzles.</summary>
    /// <param name="left">The puzzle on the left.</param>
    /// <param name="right">The puzzle on the right.</param>
    /// <returns>Whether <paramref name="left"/> does not come after <paramref name="right"/>.</returns>
    public static bool operator <=(Puzzle? left, Puzzle? right) => Compare(left, right) <= 0;

    /// <summary>Compares two puzzles.</summary>
    /// <param name="left">The puzzle on the left.</param>
    /// <param name="right">The puzzle on the right.</param>
    /// <returns>Whether <paramref name="left"/> comes after <paramref name="right"/>.</returns>
    public static bool operator >(Puzzle? left, Puzzle? right) => Compare(left, right) > 0;

    /// <summary>Compares two puzzles.</summary>
    /// <param name="left">The puzzle on the left.</param>
    /// <param name="right">The puzzle on the right.</param>
    /// <returns>Whether <paramref name="left"/> does not come before <paramref name="right"/>.</returns>
    public static bool operator >=(Puzzle? left, Puzzle? right) => Compare(left, right) >= 0;

    private static int Compare(Puzzle? left, Puzzle? right) =>
        left is null ? (right is null ? 0 : -1) : left.CompareTo(right);
}
