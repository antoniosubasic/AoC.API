using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AoC.API.Parsing;

/// <summary>
/// Reading Advent of Code's replies.
/// </summary>
/// <remarks>
/// The site has no API: everything it says arrives as a page meant for a
/// browser, and recognising it is the most fragile thing this library does. It
/// therefore lives here, with no I/O of its own, so every case can be pinned by
/// a saved response body in the test fixtures.
/// <para>
/// Nothing here fails on unexpected <em>extra</em> markup; it fails when a
/// reply says something it does not recognise at all, which is the signal that
/// the site changed and this is what needs updating.
/// </para>
/// </remarks>
internal static class Pages
{
    /// <summary>Every sample block on a puzzle page, in the order they appear.</summary>
    /// <remarks>
    /// Samples are the <c>&lt;pre&gt;&lt;code&gt;</c> blocks of the puzzle text.
    /// Emphasis markup inside them is dropped and entities are decoded, so what
    /// comes back is what the puzzle shows rather than what the source says.
    /// </remarks>
    public static string[] Samples(string html) =>
        [.. Html.AllBetween(html, "<pre><code>", "</code></pre>")
            .Select(block => Html.DecodeEntities(Html.StripTags(block)).TrimEnd('\n'))];

    /// <summary>The <paramref name="nth"/> sample block on a puzzle page, counting from one.</summary>
    /// <exception cref="ParseException">The page has fewer blocks than that.</exception>
    public static string Sample(string html, int nth)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(nth, 1);

        var samples = Samples(html);

        return nth <= samples.Length
            ? samples[nth - 1]
            : throw new ParseException(string.Create(
                CultureInfo.InvariantCulture,
                $"the puzzle page has {samples.Length} sample block(s), so there is no sample {nth}"));
    }

    /// <summary>Every answer a puzzle page shows as accepted, part one first.</summary>
    public static string[] AcceptedAnswers(string html) =>
        [.. Html.AllBetween(html, "Your puzzle answer was <code>", "</code>")
            .Select(answer => Html.DecodeEntities(Html.StripTags(answer)))];

    /// <summary>The answer a puzzle page shows as accepted for <paramref name="part"/>.</summary>
    /// <remarks>
    /// A page that shows none is what an unsolved part looks like, which is a
    /// fact about the puzzle rather than a reply that could not be read - so
    /// the caller decides what it means.
    /// </remarks>
    public static bool TryAcceptedAnswer(string html, Part part, [NotNullWhen(true)] out string? answer)
    {
        var answers = AcceptedAnswers(html);
        var index = part.Index();

        answer = index < answers.Length ? answers[index] : null;
        return answer is not null;
    }

    /// <summary>How many stars each event on the events page has been awarded.</summary>
    /// <remarks>An event with no stars yet is present with a count of zero.</remarks>
    /// <exception cref="ParseException">The page lists no events at all.</exception>
    public static SortedDictionary<Year, int> Stars(string html)
    {
        var stars = new SortedDictionary<Year, int>();
        var entries = html.Split("eventlist-event", StringSplitOptions.None);

        foreach (var entry in entries.Skip(1))
        {
            var closed = entry.IndexOf("</div>", StringComparison.Ordinal);
            var listed = closed < 0 ? entry : entry[..closed];
            if (EventYear(listed) is not { } year) { continue; }

            var counted = Html.Between(listed, "star-count\">", "</span>")?.Trim().TrimEnd('*').Trim();
            stars[year] = int.TryParse(counted, NumberStyles.None, CultureInfo.InvariantCulture, out var count)
                ? count
                : 0;
        }

        return stars.Count > 0 ? stars : throw new ParseException("the events page listed no events");
    }

    /// <summary>What the site said in reply to a submitted answer.</summary>
    /// <remarks>
    /// Matching happens on a lowercased copy so a capitalisation change cannot
    /// turn a known reply into an unrecognised one. Every needle below is
    /// lowercase for that reason, and every extracted substring comes from the
    /// same copy, so the offsets always line up.
    /// </remarks>
    /// <exception cref="ParseException">The reply is none of the ones this library knows.</exception>
    public static Submission ReadSubmission(string html)
    {
        var reply = html.ToLowerInvariant();

        if (Says(reply, "that's the right answer")) { return new Submission.Correct(); }

        if (Says(reply, "you gave an answer too recently"))
        {
            Waits.TryParseBefore(reply, " left to wait", out var wait);
            return new Submission.TooRecent(wait);
        }

        if (Says(reply, "that's not the right answer"))
        {
            var hint = Says(reply, "your answer is too high") ? Hint.TooHigh
                : Says(reply, "your answer is too low") ? Hint.TooLow
                : (Hint?)null;

            return new Submission.Incorrect(
                hint,
                Waits.TryParseBefore(reply, " before trying again", out var wait) ? wait : null);
        }

        if (Says(reply, "did you already complete it") || Says(reply, "both parts of this puzzle are complete"))
        {
            return new Submission.AlreadyComplete();
        }

        if (IsLoggedOut(reply)) { return new Submission.LoggedOut(); }

        throw new ParseException(
            $"advent of code replied to the submission with something unrecognised: {Html.Snippet(html)}");
    }

    /// <summary>Whether a reply is the site asking whoever sent it to log in.</summary>
    /// <remarks>
    /// The site answers an unauthenticated request in several ways - a short
    /// refusal on the input endpoint, a whole page with a log-in link elsewhere -
    /// and this recognises all of them.
    /// </remarks>
    public static bool IsLoggedOut(string html)
    {
        var reply = html.ToLowerInvariant();

        return Says(reply, "please log in") || Says(reply, "/auth/login");
    }

    /// <summary>Which event one entry of the events list is for.</summary>
    private static Year? EventYear(string entry)
    {
        foreach (var written in new[] { Html.Between(entry, "href=\"/", "\""), Html.Between(entry, "[", "]") })
        {
            if (written is not null
                && int.TryParse(written.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var year)
                && Year.TryOf(year, out var validated))
            {
                return validated;
            }
        }

        return null;
    }

    private static bool Says(string reply, string phrase) => reply.Contains(phrase, StringComparison.Ordinal);
}
