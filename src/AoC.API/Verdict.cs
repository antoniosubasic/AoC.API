using System.Text;
using AoC.API.Parsing;

namespace AoC.API;

/// <summary>
/// How Advent of Code judged a submitted answer.
/// </summary>
/// <remarks>
/// A rejected answer is a verdict rather than an exception: being wrong is a
/// normal outcome of solving a puzzle. The hierarchy is closed - the base
/// constructor is private, so nothing outside this file can add a case - which
/// means a <see langword="switch"/> over it can be exhaustive.
/// </remarks>
/// <example>
/// <code>
/// var message = verdict switch
/// {
///     Verdict.Correct => "gold star",
///     Verdict.Incorrect { Hint: { } hint } => $"wrong, and {hint}",
///     Verdict.Incorrect => "wrong",
///     Verdict.AlreadyComplete solved => solved.Matches ? "already solved with this" : "already solved with another",
///     Verdict.WrongLevel => "the site was not asking",
/// };
/// </code>
/// </example>
public abstract record Verdict
{
    private Verdict() { }

    /// <summary>Whether the submitted answer is the right one.</summary>
    public bool IsCorrect => this is Correct or AlreadyComplete { Matches: true };

    /// <inheritdoc />
    public sealed override string ToString() => this switch
    {
        Correct => "that's the right answer",
        Incorrect incorrect => Describe(incorrect),
        AlreadyComplete { Matches: true } => "already solved, with this answer",
        AlreadyComplete => "already solved, with a different answer",
        _ => "there is nothing to answer for this part",
    };

    private static string Describe(Incorrect incorrect)
    {
        var described = new StringBuilder("that's not the right answer");

        if (incorrect.Hint is { } hint)
        {
            described.Append("; it is ").Append(hint == Hint.TooHigh ? "too high" : "too low");
        }

        if (incorrect.Wait is { } wait)
        {
            described.Append(" (wait ").Append(Waits.Describe(wait)).Append(" before trying again)");
        }

        return described.ToString();
    }

    /// <summary>The answer was accepted.</summary>
    public sealed record Correct : Verdict;

    /// <summary>The answer was rejected.</summary>
    /// <param name="Hint">Which way the answer was wrong, when the site says.</param>
    /// <param name="Wait">How long the site asks you to wait before trying again, when it says.</param>
    public sealed record Incorrect(Hint? Hint, TimeSpan? Wait) : Verdict;

    /// <summary>
    /// The part was already solved, so the site refused to judge the submission
    /// at all.
    /// </summary>
    /// <remarks>
    /// The answer was compared against the one the puzzle page shows as
    /// accepted instead, which costs one further request.
    /// </remarks>
    /// <param name="Matches">Whether the submitted answer is the accepted one.</param>
    public sealed record AlreadyComplete(bool Matches) : Verdict;

    /// <summary>
    /// The site was not asking for an answer to that part at all.
    /// </summary>
    /// <remarks>
    /// It says the same thing in two situations and distinguishes neither: the
    /// part is not open yet, because part one is still unsolved; or the part was
    /// never a question, which is day 25's second star - that day has one
    /// puzzle, and its second star is awarded for holding the other forty-nine.
    /// Either way nothing was judged and nothing was wrong.
    /// </remarks>
    public sealed record WrongLevel : Verdict;
}
