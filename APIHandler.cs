using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace AoC.API;

public class Session
{
    private const string DOMAIN = "https://adventofcode.com";
    private readonly string _sessionCookie;
    private readonly int _year;
    private readonly int _day;

    public Session(string sessionCookie, int year, int day)
    {
        _sessionCookie = sessionCookie;
        _year = year;
        _day = day;
    }

    public Session(string sessionCookie, string input, Regex pattern)
    {
        _sessionCookie = sessionCookie;

        var match = pattern.Match(input);
        if (match.Success)
        {
            _year = int.Parse(match.Groups["year"].Value);
            _day = int.Parse(match.Groups["day"].Value);
        }
        else { throw new Exception("no regex match found"); }
    }

    private async Task<string> Get(string uri)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(uri),
            Headers =
            {
                { "Cookie", $"session={_sessionCookie}" },
            }
        };

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
    private async Task<string> Post(string uri, HttpContent? content = null)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(uri),
            Headers =
            {
                { "Cookie", $"session={_sessionCookie}" },
            }
        };

        if (content != null) { request.Content = content; }


        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    public async Task<string> GetInputText() => (await Get($"{DOMAIN}/{_year}/day/{_day}/input")).TrimEnd('\n');
    public async Task<string[]> GetInputLines() => (await GetInputText()).Split('\n');
    public async Task<Dictionary<int, int>> GetAllStars()
    {
        var response = (await Get($"{DOMAIN}/events"))
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
    public async Task<int> GetThisYearStars() => (await GetAllStars())[_year];
    public async Task<bool> SubmitAnswer(int level, object answer)
    {
        var content = new StringContent($"level={level}&answer={answer}")
        {
            Headers =
            {
                ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded")
            }
        };

        var response = await Post($"{DOMAIN}/{_year}/day/{_day}/answer", content);
        return response.Contains("That's the right answer!");
    }
}
