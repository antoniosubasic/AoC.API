using System.Globalization;
using System.Text;

namespace AoC.API.Parsing;

/// <summary>
/// The waits the site phrases in words, as <see cref="TimeSpan"/>s.
/// </summary>
/// <remarks>
/// The site writes a remaining cooldown two ways - compactly, as
/// <c>4m 30s</c>, and in prose, as <c>one minute</c> or <c>5 minutes</c>. Both
/// are read here, so a caller gets something it can actually wait for rather
/// than the site's own words.
/// </remarks>
internal static class Waits
{
    private const long Second = 1;
    private const long Minute = 60 * Second;
    private const long Hour = 60 * Minute;
    private const long Day = 24 * Hour;

    /// <summary>A century, which is longer than any wait the site has ever asked for.</summary>
    private const long MaxSeconds = 100 * 365 * Day;

    /// <summary>Reads a wait out of <paramref name="text"/>.</summary>
    /// <param name="text">The words the site used.</param>
    /// <param name="wait">The wait, when there is one.</param>
    /// <returns><see langword="true"/> if the text names a wait at all.</returns>
    public static bool TryParse(string text, out TimeSpan wait)
    {
        var total = 0L;
        var pending = (long?)null;
        var found = false;

        foreach (var word in Words(text))
        {
            if (TryCount(word, out var counted))
            {
                pending = counted;
            }
            else if (TryUnit(word, out var unit))
            {
                if (pending is { } count)
                {
                    total = Add(total, count, unit);
                    pending = null;
                    found = true;
                }
            }
            else if (TryCompact(word, out var compact))
            {
                total = Add(total, compact, Second);
                found = true;
            }
        }

        wait = found ? TimeSpan.FromSeconds(total) : TimeSpan.Zero;
        return found;
    }

    /// <summary>The wait written immediately before <paramref name="marker"/>.</summary>
    /// <remarks>
    /// Anchoring on the end of the phrase rather than its beginning keeps this
    /// working whichever way the sentence leading up to it is worded - the site
    /// has several, and they all end the same way.
    /// </remarks>
    /// <param name="reply">The reply, lowercased.</param>
    /// <param name="marker">The phrase the wait is written before.</param>
    /// <param name="wait">The wait, when there is one.</param>
    /// <returns><see langword="true"/> if a wait was written there.</returns>
    public static bool TryParseBefore(string reply, string marker, out TimeSpan wait)
    {
        const int lastWords = 6;

        var at = reply.IndexOf(marker, StringComparison.Ordinal);
        if (at < 0)
        {
            wait = TimeSpan.Zero;
            return false;
        }

        var words = Words(reply[..at]);
        var tail = words.Count > lastWords ? words.GetRange(words.Count - lastWords, lastWords) : words;

        return TryParse(string.Join(' ', tail), out wait);
    }

    /// <summary>Renders a wait the way the site phrases it, for a message.</summary>
    /// <param name="wait">The wait.</param>
    /// <returns>The wait, written as the site writes it.</returns>
    public static string Describe(TimeSpan wait)
    {
        var seconds = (long)Math.Max(0, wait.TotalSeconds);
        var described = new StringBuilder();

        void Append(long count, char unit)
        {
            if (described.Length > 0) { described.Append(' '); }
            described.Append(count.ToString(CultureInfo.InvariantCulture)).Append(unit);
        }

        if (seconds / Hour > 0) { Append(seconds / Hour, 'h'); }
        if (seconds / Minute % 60 > 0) { Append(seconds / Minute % 60, 'm'); }
        if (seconds % 60 > 0 || described.Length == 0) { Append(seconds % 60, 's'); }

        return described.ToString();
    }

    /// <summary>The words of a text, stripped of anything that is not alphanumeric.</summary>
    private static List<string> Words(string text)
    {
        var words = new List<string>();

        foreach (var word in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = word.AsSpan();
            while (!trimmed.IsEmpty && !char.IsAsciiLetterOrDigit(trimmed[0])) { trimmed = trimmed[1..]; }
            while (!trimmed.IsEmpty && !char.IsAsciiLetterOrDigit(trimmed[^1])) { trimmed = trimmed[..^1]; }

            if (!trimmed.IsEmpty) { words.Add(trimmed.ToString().ToLowerInvariant()); }
        }

        return words;
    }

    /// <summary>A number, written as digits or as one of the words the site uses.</summary>
    private static bool TryCount(string word, out long count)
    {
        if (word.Length > 0 && word.All(char.IsAsciiDigit))
        {
            // A wait longer than a century is not one; clamping keeps a silly
            // number from reading as no number at all.
            count = long.TryParse(word, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : MaxSeconds;
            return true;
        }

        count = word switch
        {
            "one" or "a" or "an" => 1,
            "two" => 2,
            "three" => 3,
            "four" => 4,
            "five" => 5,
            "six" => 6,
            "seven" => 7,
            "eight" => 8,
            "nine" => 9,
            "ten" => 10,
            _ => 0,
        };

        return count != 0;
    }

    /// <summary>A unit of time, written out.</summary>
    private static bool TryUnit(string word, out long unit)
    {
        unit = word switch
        {
            "second" or "seconds" => Second,
            "minute" or "minutes" => Minute,
            "hour" or "hours" => Hour,
            "day" or "days" => Day,
            _ => 0,
        };

        return unit != 0;
    }

    /// <summary>A count and its unit written together, as in <c>4m</c> or <c>30s</c>.</summary>
    private static bool TryCompact(string word, out long seconds)
    {
        seconds = 0;

        var digits = 0;
        while (digits < word.Length && char.IsAsciiDigit(word[digits])) { digits++; }

        if (digits == 0 || digits == word.Length || !TryCount(word[..digits], out var count)) { return false; }

        var unit = word[digits..] switch
        {
            "s" => Second,
            "m" => Minute,
            "h" => Hour,
            "d" => Day,
            _ => 0,
        };

        if (unit == 0) { return false; }

        seconds = Add(0, count, unit);
        return true;
    }

    /// <summary>Adds <paramref name="count"/> of <paramref name="unit"/>, clamping rather than wrapping.</summary>
    private static long Add(long total, long count, long unit)
    {
        if (unit <= 0 || count > MaxSeconds / unit) { return MaxSeconds; }

        var sum = total + (count * unit);
        return sum > MaxSeconds ? MaxSeconds : sum;
    }
}
