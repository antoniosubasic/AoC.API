using System.Text;

namespace AoC.API.Parsing;

/// <summary>
/// The little bit of HTML reading this library does, by hand.
/// </summary>
/// <remarks>
/// Advent of Code sends pages meant for a browser, and picking the handful of
/// values this library needs out of them does not call for a parser that
/// understands HTML - nor for a regular expression, which is what the previous
/// version leaned on and what made a wording change fail in an unreadable way.
/// Every needle below is a literal the site writes, matched ordinally.
/// </remarks>
internal static class Html
{
    /// <summary>The text between the first <paramref name="start"/> and the next <paramref name="end"/> after it.</summary>
    public static string? Between(string haystack, string start, string end)
    {
        var opening = haystack.IndexOf(start, StringComparison.Ordinal);
        if (opening < 0) { return null; }

        var from = opening + start.Length;
        var closing = haystack.IndexOf(end, from, StringComparison.Ordinal);

        return closing < 0 ? null : haystack[from..closing];
    }

    /// <summary>The text between every <paramref name="start"/> and the next <paramref name="end"/> after it.</summary>
    public static List<string> AllBetween(string haystack, string start, string end)
    {
        var found = new List<string>();
        var at = 0;

        while (at < haystack.Length)
        {
            var opening = haystack.IndexOf(start, at, StringComparison.Ordinal);
            if (opening < 0) { break; }

            var from = opening + start.Length;
            var closing = haystack.IndexOf(end, from, StringComparison.Ordinal);
            if (closing < 0) { break; }

            found.Add(haystack[from..closing]);
            at = closing + end.Length;
        }

        return found;
    }

    /// <summary>Drops every tag, keeping the text between them.</summary>
    public static string StripTags(string html)
    {
        var text = new StringBuilder(html.Length);
        var depth = 0;

        foreach (var character in html)
        {
            switch (character)
            {
                case '<':
                    depth++;
                    break;
                case '>' when depth > 0:
                    depth--;
                    break;
                default:
                    if (depth == 0) { text.Append(character); }
                    break;
            }
        }

        return text.ToString();
    }

    /// <summary>Decodes the handful of entities the puzzle text uses.</summary>
    public static string DecodeEntities(string text)
    {
        var decoded = new StringBuilder(text.Length);
        var at = 0;

        while (at < text.Length)
        {
            var ampersand = text.IndexOf('&', at);
            if (ampersand < 0)
            {
                decoded.Append(text, at, text.Length - at);
                break;
            }

            decoded.Append(text, at, ampersand - at);

            var semicolon = text.IndexOf(';', ampersand);
            var entity = semicolon > 0 && semicolon - ampersand <= 8 ? Decode(text[ampersand..(semicolon + 1)]) : null;

            if (entity is null)
            {
                decoded.Append('&');
                at = ampersand + 1;
            }
            else
            {
                decoded.Append(entity);
                at = semicolon + 1;
            }
        }

        return decoded.ToString();
    }

    /// <summary>The opening of a reply, with markup stripped, for an error message.</summary>
    public static string Snippet(string html)
    {
        const int limit = 160;

        var text = DecodeEntities(StripTags(html));
        var snippet = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        return snippet.Length > limit ? string.Concat(snippet.AsSpan(0, limit - 3), "...") : snippet;
    }

    private static string? Decode(string entity) => entity switch
    {
        "&amp;" => "&",
        "&lt;" => "<",
        "&gt;" => ">",
        "&quot;" => "\"",
        "&apos;" or "&#39;" => "'",
        "&nbsp;" => " ",
        _ => null,
    };
}
