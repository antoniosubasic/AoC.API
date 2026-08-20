using System.Globalization;

namespace AoC.API;

/// <summary>
/// Coordinates that do not name a puzzle.
/// </summary>
/// <remarks>
/// This is an <see cref="ArgumentException"/> rather than an
/// <see cref="AdventOfCodeException"/> on purpose: nothing was asked of the
/// site, and nothing will be. A coordinate is checked where it is written, so
/// an impossible one never becomes a request.
/// </remarks>
public sealed class PuzzleException : ArgumentException
{
    /// <summary>Initializes a new instance of the <see cref="PuzzleException"/> class.</summary>
    public PuzzleException() : base("the coordinates do not name a puzzle") { }

    /// <summary>Initializes a new instance of the <see cref="PuzzleException"/> class.</summary>
    /// <param name="message">Which coordinate is wrong, and why.</param>
    public PuzzleException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="PuzzleException"/> class.</summary>
    /// <param name="message">Which coordinate is wrong, and why.</param>
    /// <param name="innerException">What went wrong underneath.</param>
    public PuzzleException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>Initializes a new instance of the <see cref="PuzzleException"/> class.</summary>
    /// <param name="message">Which coordinate is wrong, and why.</param>
    /// <param name="paramName">The parameter the rejected coordinate came from.</param>
    public PuzzleException(string message, string? paramName) : base(message, paramName) { }

    internal static PuzzleException ForYear(int year, string paramName) => new(
        FormattableString.Invariant($"advent of code started in {Year.First}, so there is no {year} event"),
        paramName);

    internal static PuzzleException ForDay(int day, string paramName) => new(
        FormattableString.Invariant(
            $"advent of code publishes puzzles on days {Day.First} to {Day.LastFull}, so there is no day {day}"),
        paramName);

    internal static PuzzleException ForPairing(Year year, Day day, string paramName) => new(
        string.Create(
            CultureInfo.InvariantCulture,
            $"the {year} event stops after day {year.LastDay}, so there is no day {day}"),
        paramName);

    internal static PuzzleException ForPart(int part, string paramName) => new(
        FormattableString.Invariant($"a puzzle has two parts, so there is no part {part}"),
        paramName);
}
