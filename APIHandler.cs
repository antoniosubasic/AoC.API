using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace AoC.API;

/// <summary>
/// Represents an API-Session for the AoC puzzle
/// </summary>
public partial class Session
{
    /// <summary>
    /// The session cookie for authentication
    /// </summary>
    private string _cookie { get; }

    /// <summary>
    /// The year of the AoC puzzle
    /// </summary>
    private int _year { get; }

    /// <summary>
    /// The day of the AoC puzzle
    /// </summary>
    private int _day { get; }

    /// <summary>
    /// Initializes a new Session instance
    /// </summary>
    /// <param name="cookie">The session cookie - How to obtain: https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication</param>
    /// <param name="year">The year of the AoC puzzle</param>
    /// <param name="day">The day of the AoC puzzle</param>
    public Session(string cookie, int year, int day)
    {
        _cookie = cookie;
        _year = year;
        _day = day;
    }

    /// <summary>
    /// Initializes a new Session instance
    /// </summary>
    /// <param name="cookie">The session cookie - How to obtain: https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication</param>
    /// <param name="input">The input string containing year and day</param>
    /// <param name="pattern">The regex pattern to extract year and day - group containing year must be named "year" and group containing day must be named "day" - How to name regex groups: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions/Named_capturing_group</param>
    /// <exception cref="RegexMatchException">Thrown when the regex pattern does not match the input string</exception> 
    public Session(string cookie, string input, Regex pattern)
    {
        _cookie = cookie;

        var match = pattern.Match(input);
        if (match.Success)
        {
            _year = int.Parse(match.Groups["year"].Value);
            _day = int.Parse(match.Groups["day"].Value);
        }
        else { throw new RegexMatchException(); }
    }


    /// <summary>
    /// Sends an HTTP request
    /// </summary>
    /// <param name="method">The HTTP method of the request</param>
    /// <param name="uri">The URI of the request</param>
    /// <param name="content">The HTTP content of the request (optional)</param>
    /// <returns>The response as a string</returns>
    private async Task<string> SendRequestAsync(HttpMethod method, string uri, HttpContent? content = null)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(uri),
            Content = content,
            Headers = { { "Cookie", $"session={_cookie}" } }
        };

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Retrieves the nth sample input text of the AoC puzzle
    /// </summary>
    /// <param name="nth">The nth sample to retreive</param>
    /// <returns>The sample input text as a string</returns>
    /// <exception cref="RegexMatchException">Thrown when the sample could not be retrieved</exception>
    public async Task<string> GetSampleInputTextAsync(int nth)
    {
        var response = await SendRequestAsync(HttpMethod.Get, $"https://adventofcode.com/{_year}/day/{_day}");
        var matches = SampleRegex().Matches(response);

        if (matches.Count >= nth) { return matches[nth - 1].Groups["sample"].Value.TrimEnd('\n'); }
        else { throw new RegexMatchException("sample could not be retrieved"); }
    }

    /// <summary>
    /// Retrieves the nth sample input lines of the AoC puzzle
    /// </summary>
    /// <param name="nth">The nth sample to retreive</param>
    /// <returns>The sample input lines as a string array</returns>
    public async Task<string[]> GetSampleInputLinesAsync(int nth) => (await GetSampleInputTextAsync(nth)).Split('\n');

    /// <summary>
    /// Retrieves the input text of the AoC puzzle
    /// </summary>
    /// <returns>The input text as a string</returns>
    public async Task<string> GetInputTextAsync() => (await SendRequestAsync(HttpMethod.Get, $"https://adventofcode.com/{_year}/day/{_day}/input")).TrimEnd('\n');

    /// <summary>
    /// Retrieves the input lines of the AoC puzzle
    /// </summary>
    /// <returns>The input lines as a string array</returns>
    public async Task<string[]> GetInputLinesAsync() => (await GetInputTextAsync()).Split('\n');

    /// <summary>
    /// Retrieves each year's number of stars earned
    /// </summary>
    /// <returns>A dictionary with the year as the key and the number of stars earned as the value</returns>
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

            return (year, stars);
        }).ToDictionary(item => item.year, item => item.stars);
    }

    /// <summary>
    /// Submits an answer to part 1 or 2 of the AoC puzzle
    /// </summary>
    /// <param name="part">The part of the puzzle - 1 or 2</param>
    /// <param name="answer">The answer to submit</param>
    /// <returns>Returns a Response type</returns>
    /// <exception cref="RegexMatchException">Thrown when the some data could not be retrieved</exception>
    /// <exception cref="UnknownResponseException">Thrown when the response is unknown</exception>
    public async Task<Response> SubmitAnswerAsync(int part, object answer)
    {
        var content = new StringContent($"level={part}&answer={answer}")
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        var response = await SendRequestAsync(HttpMethod.Post, $"https://adventofcode.com/{_year}/day/{_day}/answer", content);

        if (response.Contains("That's the right answer!")) { return true; }
        else if (response.Contains("Did you already complete it?") || response.Contains("Both parts of this puzzle are complete!"))
        {
            var dayResponse = await SendRequestAsync(HttpMethod.Get, $"https://adventofcode.com/{_year}/day/{_day}");
            var matches = PuzzleAnswerRegex().Matches(dayResponse);

            if (matches.Count >= part) { return matches[part - 1].Groups["answer"].Value == answer.ToString(); }
            else { throw new RegexMatchException("answer could not be retrieved"); }
        }
        else if (response.Contains("You gave an answer too recently"))
        {
            var match = TimeForAnswerTooRecentRegex().Match(response);

            if (match.Success) { return match.Groups["time"].Value; }
            else { throw new RegexMatchException("time could not be retrieved"); }
        }
        else if (response.Contains("That's not the right answer.") || response.Contains("before trying again."))
        {
            var match = TimeForWrongAnswerRegex().Match(response);

            if (match.Success) { return (false, match.Groups["time"].Value); }
            else { return false; }
        }
        else { throw new UnknownResponseException(); }
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

/// <summary>
/// A response with a success status and a cooldown period
/// </summary>
public class Response
{
    /// <summary>
    /// Value indicating whether the operation was successful
    /// </summary>
    /// <value>True if successful; False if the unsuccessful; null if on cooldown</value>
    public bool? Success { get; private set; }

    /// <summary>
    /// Remaining cooldown period
    /// </summary>
    /// <value>The cooldown period, or null if no cooldown</value>
    public string? Cooldown { get; private set; }

    /// <summary>
    /// Implicitly converts a boolean value to a Response type
    /// </summary>
    /// <param name="value">The boolean value to convert</param>
    public static implicit operator Response(bool value) => new() { Success = value };

    /// <summary>
    /// Implicitly converts a string value to a Response type
    /// </summary>
    /// <param name="cooldown">The string value to convert</param>
    public static implicit operator Response(string cooldown) => new() { Cooldown = cooldown };


    /// <summary>
    /// Implicitly converts a tuple value of bool and string to a Response type
    /// </summary>
    /// <param name="t">The tuple value to convert</param>
    public static implicit operator Response((bool value, string cooldown) t) => new() { Success = t.value, Cooldown = t.cooldown };

    /// <summary>
    /// Returns a string representation of the Response
    /// </summary>
    /// <returns>A string representation of the Response</returns>
    public override string ToString()
        => $"{(Success is not null ? Success : string.Empty)}{(Success is not null && Cooldown is not null ? "\n" : string.Empty)}{(Cooldown is not null ? $"on cooldown: {Cooldown}" : string.Empty)}";
}

public class RegexMatchException : Exception
{
    public RegexMatchException() : base("no regex match") { }
    public RegexMatchException(string message) : base(message) { }
    public RegexMatchException(string message, Exception inner) : base(message, inner) { }
}

public class UnknownResponseException : Exception
{
    public UnknownResponseException() : base("unknown response") { }
    public UnknownResponseException(string message) : base(message) { }
    public UnknownResponseException(string message, Exception inner) : base(message, inner) { }
}
