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


    private async Task<string> SendRequest(HttpMethod method, string uri, HttpContent? content = null)
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

    public async Task<string> GetInputText() => (await SendRequest(HttpMethod.Get, $"https://www.adventofcode.com/{_year}/day/{_day}/input")).TrimEnd('\n');
    public async Task<string[]> GetInputLines() => (await GetInputText()).Split('\n');

    public async Task<Dictionary<int, int>> GetAllStars()
    {
        var response = (await SendRequest(HttpMethod.Get, $"https://www.adventofcode.com/events"))
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

    public async Task<bool> SubmitAnswer(int part, object answer)
    {
        var content = new StringContent($"level={part}&answer={answer}")
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        var response = await SendRequest(HttpMethod.Post, $"https://www.adventofcode.com/{_year}/day/{_day}/answer", content);

        if (response.Contains("That's the right answer!")) { return true; }
        else if (response.Contains("Your puzzle answer was"))
        {
            var regex = new Regex(@"<p>Your puzzle answer was <code>(?<answer>.*?)</code>.*?</p>");
            var matches = regex.Matches(response);
            return matches.Count >= part && answer.ToString() == matches[part - 1].Groups["answer"].Value;
        }
        else { return false; }
    }
}
