using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace AoC.API;

public partial class Session
{
    /// <summary>
    /// The cookie value for authentication.
    /// </summary>
    private string _cookie { get; }

    /// <summary>
    /// The year of the Advent of Code puzzle.
    /// </summary>
    private int _year { get; }

    /// <summary>
    /// The day of the Advent of Code puzzle.
    /// </summary>
    private int _day { get; }

    /// <summary>
    /// Initializes a new instance of the session class with a specified cookie, year and day.
    /// </summary>
    /// <param name="cookie">The session cookie - How to obtain the session cookie: https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication</param>
    /// <param name="year">The year of the Advent of Code puzzle.</param>
    /// <param name="day">The day of the Advent of Code puzzle.</param>
    public Session(string cookie, int year, int day)
    {
        _cookie = cookie;
        _year = year;
        _day = day;
    }

    /// <summary>
    /// Initializes a new instance of the session class with a specified cookie, input string and regex pattern.
    /// </summary>
    /// <param name="cookie">The session cookie - How to obtain the session cookie: https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication</param>
    /// <param name="input">The input string.</param>
    /// <param name="pattern">The regex pattern - group containing year must be named "year" and group containing day must be named "day". How to name regex groups: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions/Named_capturing_group</param>
    public Session(string cookie, string input, Regex pattern)
    {
        _cookie = cookie;

        var match = pattern.Match(input);
        if (match.Success)
        {
            _year = int.Parse(match.Groups["year"].Value);
            _day = int.Parse(match.Groups["day"].Value);
        }
        else { throw new Exception("no regex match found"); }
    }


    /// <summary>
    /// Sends an HTTP request.
    /// </summary>
    /// <param name="method">The HTTP method to use for the request.</param>
    /// <param name="uri">The URI of the request.</param>
    /// <param name="content">The HTTP content to send with the request (optional).</param>
    /// <returns>The response content as a string.</returns>
    private async Task<string> SendRequestAsync(HttpMethod method, string uri, HttpContent? content = null)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(uri),
            Headers = { { "Cookie", $"session={_cookie}" } }
        };
        if (content != null) { request.Content = content; }

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    /// <summary>
    /// Retrieves the nth sample input text of the Advent of Code puzzle.
    /// </summary>
    /// <param name="nth">The nth sample of the Advent of Code puzzle.</param>
    /// <returns>The sample input text as a string.</returns>
    public async Task<string> GetSampleInputTextAsync(int nth)
    {
        var response = await SendRequestAsync(HttpMethod.Get, $"https://adventofcode.com/{_year}/day/{_day}");
        var matches = SampleRegex().Matches(response);

        if (matches.Count >= nth) { return matches[nth - 1].Groups["sample"].Value.TrimEnd('\n'); }
        else { throw new Exception("sample could not be found"); }
    }

    /// <summary>
    /// Retrieves the nth sample input lines of the Advent of Code puzzle.
    /// </summary>
    /// <param name="nth">The nth sample of the Advent of Code puzzle.</param>
    /// <returns>The sample input lines as a string array.</returns>
    public async Task<string[]> GetSampleInputLinesAsync(int nth) => (await GetSampleInputTextAsync(nth)).Split('\n');

    /// <summary>
    /// Retrieves the input text for the Advent of Code puzzle.
    /// </summary>
    /// <returns>The input text as a string.</returns>
    public async Task<string> GetInputTextAsync() => (await SendRequestAsync(HttpMethod.Get, $"https://adventofcode.com/{_year}/day/{_day}/input")).TrimEnd('\n');

    /// <summary>
    /// Retrieves the input lines for the Advent of Code puzzle.
    /// </summary>
    /// <returns>The input lines as a string array.</returns>
    public async Task<string[]> GetInputLinesAsync() => (await GetInputTextAsync()).Split('\n');

    /// <summary>
    /// Retrieves the number of stars earned for each year's Advent of Code.
    /// </summary>
    /// <returns>A dictionary containing the year as the key and the number of stars earned as the value.</returns>
    public async Task<Dictionary<int, int>> GetAllStarsAsync()
    {
        var response = (await SendRequestAsync(HttpMethod.Get, $"https://adventofcode.com/events"))
            .Split('\n')
            .Where(line => line.StartsWith("<div class=\"eventlist-event\">"))
            .ToArray();

        return response.Select(line =>
        {
            var yearIndex = line.IndexOf("</a>") - 5;
            var starIndex = line.IndexOf("</span>") - 3;

            var year = int.Parse(line.Substring(yearIndex, 4));
            var stars = starIndex < 0 ? 0 : int.Parse(line.Substring(starIndex, 2));

            return new { Year = year, Stars = stars };
        }).ToDictionary(x => x.Year, x => x.Stars);
    }

    /// <summary>
    /// Submits the answer for a specific part of the Advent of Code puzzle.
    /// </summary>
    /// <param name="part">The part of the puzzle.</param>
    /// <param name="answer">The answer to submit.</param>
    /// <returns>Returns whether the answer was true or false and a cooldown if existent.</returns>
    public async Task<Response> SubmitAnswerAsync(int part, object answer)
    {
        var content = new StringContent($"level={part}&answer={answer}")
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        var response = await SendRequestAsync(HttpMethod.Post, $"https://adventofcode.com/{_year}/day/{_day}/answer", content);

        if (response.Contains("That's the right answer!")) { return true; }
        else if (response.Contains("You don't seem to be solving the right level.  Did you already complete it?"))
        {
            var dayResponse = await SendRequestAsync(HttpMethod.Get, $"https://adventofcode.com/{_year}/day/{_day}");
            var matches = PuzzleAnswerRegex().Matches(dayResponse);

            if (matches.Count >= part) { return matches[part - 1].Groups["answer"].Value == answer.ToString(); }
            else { throw new Exception("answer could not be found"); }
        }
        else if (response.Contains("You gave an answer too recently"))
        {
            var match = TimeForAnswerTooRecentRegex().Match(response);

            if (match.Success) { return match.Groups["time"].Value; }
            else { throw new Exception("time could not be found"); }
        }
        else if (response.Contains("That's not the right answer.") && response.Contains("before trying again."))
        {
            var match = TimeForWrongAnswerRegex().Match(response);

            if (match.Success) { return (false, match.Groups["time"].Value); }
            else { throw new Exception("time could not be found"); }
        }
        else { return false; }
    }

    [GeneratedRegex(@"<pre><code>(?<sample>(.*?\n)*?)<\/code><\/pre>")]
    private static partial Regex SampleRegex();

    [GeneratedRegex(@"<p>Your puzzle answer was <code>(?<answer>.*?)</code>.</p>")]
    private static partial Regex PuzzleAnswerRegex();

    [GeneratedRegex(@"You have (?<time>.*?) left to wait")]
    private static partial Regex TimeForAnswerTooRecentRegex();

    [GeneratedRegex(@"wait (?<time>.*?) before trying again")]
    private static partial Regex TimeForWrongAnswerRegex();
}

public class Response
{
    public bool? Value { get; private set; }
    public string? Cooldown { get; private set; }

    public static implicit operator Response(bool value) => new() { Value = value };
    public static implicit operator Response(string cooldown) => new() { Cooldown = cooldown };
    public static implicit operator Response((bool value, string cooldown) t) => new() { Value = t.value, Cooldown = t.cooldown };

    public override string ToString()
        => $"{(Value is not null ? Value : string.Empty)}{(Value is not null && Cooldown is not null ? "\n" : string.Empty)}{(Cooldown is not null ? $"on cooldown: {Cooldown}" : string.Empty)}";
}
