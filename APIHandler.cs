using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace AoC.API;

public class Session
{
    private string _rawCookie { get; }
    private string _cookie => $"session={_rawCookie}";
    private int _year { get; }
    private int _day { get; }

    public Session(string cookie, int year, int day)
    {
        _rawCookie = cookie;
        _year = year;
        _day = day;
    }

    public Session(string cookie, string input, Regex pattern)
    {
        _rawCookie = cookie;

        var match = pattern.Match(input);
        if (match.Success)
        {
            _year = int.Parse(match.Groups["year"].Value);
            _day = int.Parse(match.Groups["day"].Value);
        }
        else { throw new Exception("no regex match found"); }
    }


    private async Task<string> SendRequestAsync(HttpMethod method, string uri, HttpContent? content = null)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(uri),
            Headers = { { "Cookie", _cookie } }
        };
        if (content != null) { request.Content = content; }

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    public async Task<string> GetSampleInputTextAsync(int part = 1)
    {
        var response = await SendRequestAsync(HttpMethod.Get, $"https://adventofcode.com/{_year}/day/{_day}");

        var regex = new Regex(@"<pre><code>(?<sample>(.*?\n)*?)<\/code><\/pre>");
        var matches = regex.Matches(response);

        if (matches.Count >= part) { return matches[part - 1].Groups["sample"].Value.TrimEnd('\n'); }
        else { throw new Exception("sample could not be found"); }
    }
    public async Task<string[]> GetSampleInputLinesAsync(int part = 1) => (await GetSampleInputTextAsync(part)).Split('\n');

    public async Task<string> GetInputTextAsync() => (await SendRequestAsync(HttpMethod.Get, $"https://adventofcode.com/{_year}/day/{_day}/input")).TrimEnd('\n');
    public async Task<string[]> GetInputLinesAsync() => (await GetInputTextAsync()).Split('\n');

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

    public async Task<string> SubmitAnswerAsync(int part, object answer)
    {
        var content = new StringContent($"level={part}&answer={answer}")
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        var response = await SendRequestAsync(HttpMethod.Post, $"https://adventofcode.com/{_year}/day/{_day}/answer", content);

        if (response.Contains("That's the right answer!") || response.Contains("You don't seem to be solving the right level.  Did you already complete it?")) { return "True"; }
        else if (response.Contains("You gave an answer too recently") || response.Contains("before trying again"))
        {
            var regex = (
                new Regex(@"You have (?<time>.*?) left to wait"),
                new Regex(@"wait (?<time>.*?) before trying again")
            );

            var match = (
                regex.Item1.Match(response),
                regex.Item2.Match(response)
            );

            return $"on cooldown: {(match.Item1.Success ? match.Item1 : match.Item2).Groups["time"].Value}";
        }
        else { return "False"; }
    }
}
