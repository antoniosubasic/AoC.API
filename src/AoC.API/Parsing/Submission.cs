namespace AoC.API.Parsing;

/// <summary>
/// What the site said about a submitted answer.
/// </summary>
/// <remarks>
/// This is the reply as written, before it means anything: turning it into a
/// <see cref="Verdict"/> is <see cref="Session.SubmitAnswerAsync"/>'s job,
/// because two of these need a second request to resolve.
/// </remarks>
internal abstract record Submission
{
    private Submission() { }

    /// <summary><c>That's the right answer!</c></summary>
    public sealed record Correct : Submission;

    /// <summary><c>That's not the right answer.</c></summary>
    /// <param name="Hint">Which way the answer was wrong, when the site says.</param>
    /// <param name="Wait">How long to wait before trying again, when the site says.</param>
    public sealed record Incorrect(Hint? Hint, TimeSpan? Wait) : Submission;

    /// <summary><c>You gave an answer too recently.</c> Nothing was judged.</summary>
    /// <param name="Wait">How much of the wait is left.</param>
    public sealed record TooRecent(TimeSpan Wait) : Submission;

    /// <summary><c>You don't seem to be solving the right level. Did you already complete it?</c></summary>
    public sealed record AlreadyComplete : Submission;

    /// <summary>The reply is a page asking whoever sent it to log in.</summary>
    public sealed record LoggedOut : Submission;
}
