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
    /// <returns>Returns a SubmissionResult indicating the outcome</returns>
    /// <exception cref="RegexMatchException">Thrown when the some data could not be retrieved</exception>
    /// <exception cref="UnknownResponseException">Thrown when the response is unknown</exception>
    public async Task<SubmissionResult> SubmitAnswerAsync(int part, object answer)
    {
        var content = new StringContent($"level={part}&answer={answer}")
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        var response = await SendRequestAsync(HttpMethod.Post, $"https://adventofcode.com/{_year}/day/{_day}/answer", content);

        // correct
        if (response.Contains("That's the right answer!"))
        {
            return new SubmissionResult(SubmissionStatus.Correct);
        }

        //  already submitted before - check if the stored answer matches currently submitted one
        if (response.Contains("Did you already complete it?") || response.Contains("Both parts of this puzzle are complete!"))
        {
            var dayResponse = await SendRequestAsync(HttpMethod.Get, $"https://adventofcode.com/{_year}/day/{_day}");
            var matches = PuzzleAnswerRegex().Matches(dayResponse);

            if (matches.Count >= part)
            {
                var isCorrect = matches[part - 1].Groups["answer"].Value == answer.ToString();
                return new SubmissionResult(isCorrect ? SubmissionStatus.Correct : SubmissionStatus.Incorrect);
            }
            else
            {
                throw new RegexMatchException("answer could not be retrieved");
            }
        }

        // answer submitted too recently - on cooldown
        if (response.Contains("You gave an answer too recently"))
        {
            var match = TimeForAnswerTooRecentRegex().Match(response);
            var cooldownTime = match.Success ? match.Groups["time"].Value : null;

            if (!match.Success) { throw new RegexMatchException("time could not be retrieved"); }

            return new SubmissionResult(SubmissionStatus.OnCooldown, cooldownTime);
        }

        // wrong answer
        if (response.Contains("That's not the right answer.") || response.Contains("before trying again."))
        {
            var match = TimeForWrongAnswerRegex().Match(response);
            var cooldownTime = match.Success ? match.Groups["time"].Value : null;

            return new SubmissionResult(SubmissionStatus.Incorrect, cooldownTime);
        }

        throw new UnknownResponseException();
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
/// Represents the status of an answer submission
/// </summary>
public enum SubmissionStatus
{
    /// <summary>
    /// The answer was correct
    /// </summary>
    Correct,

    /// <summary>
    /// The answer was incorrect
    /// </summary>
    Incorrect,

    /// <summary>
    /// The user is on cooldown and must wait before submitting again
    /// </summary>
    OnCooldown
}

/// <summary>
/// Represents the result of submitting an answer to an Advent of Code puzzle
/// </summary>
/// <remarks>
/// Initializes a new SubmissionResult
/// </remarks>
/// <param name="status">The submission status</param>
/// <param name="cooldownTime">The cooldown time (optional)</param>
public class SubmissionResult(SubmissionStatus status, string? cooldownTime = null)
{
    /// <summary>
    /// The status of the submission
    /// </summary>
    public SubmissionStatus Status { get; } = status;

    /// <summary>
    /// The cooldown time remaining (only relevant when Status is OnCooldown or Incorrect with cooldown)
    /// </summary>
    public string? CooldownTime { get; } = cooldownTime;

    /// <summary>
    /// Gets whether the submission was successful
    /// </summary>
    public bool IsCorrect => Status == SubmissionStatus.Correct;

    /// <summary>
    /// Gets whether the user is currently on cooldown
    /// </summary>
    public bool HasCooldown => !string.IsNullOrEmpty(CooldownTime);

    /// <summary>
    /// Returns a string representation of the submission result
    /// </summary>
    /// <returns>A string representation of the SubmissionResult</returns>
    public override string ToString()
    {
        return Status switch
        {
            SubmissionStatus.Correct => "Correct answer!",
            SubmissionStatus.Incorrect when HasCooldown => $"Incorrect answer. Wait {CooldownTime} before trying again.",
            SubmissionStatus.Incorrect => "Incorrect answer.",
            SubmissionStatus.OnCooldown => $"On cooldown. {CooldownTime} remaining.",
            _ => throw new InvalidOperationException("Invalid submission status")
        };
    }
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
