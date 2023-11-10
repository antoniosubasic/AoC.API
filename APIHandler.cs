using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace AoC.API;

public class Session
{
    private const string DOMAIN = "https://adventofcode.com";

    public string SessionCookie { get; private set; }
    public int Year { get; private set; }
    public int Day { get; private set; }

    public Session(string sessionCookie, int year, int day)
    {
        SessionCookie = sessionCookie;
        Year = year;
        Day = day;
    }

    public Session(string sessionCookie, string input, Regex pattern)
    {
        SessionCookie = sessionCookie;

        var match = pattern.Match(input);
        if (match.Success)
        {
            Year = int.Parse(match.Groups["year"].Value);
            Day = int.Parse(match.Groups["day"].Value);
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
            Headers = { { "Cookie", $"session={SessionCookie}" } }
        };
        if (content != null) { request.Content = content; }

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    public async Task<string> GetInputText() => (await SendRequest(HttpMethod.Get, $"{DOMAIN}/{Year}/day/{Day}/input")).TrimEnd('\n');
    public async Task<string[]> GetInputLines() => (await GetInputText()).Split('\n');

    public async Task<Dictionary<int, int>> GetAllStars()
    {
        var response = (await SendRequest(HttpMethod.Get, $"{DOMAIN}/events"))
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

    public async Task<bool> SubmitAnswer(int level, object answer)
    {
        var content = new StringContent($"level={level}&answer={answer}")
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded") }
        };

        var response = await SendRequest(HttpMethod.Post, $"{DOMAIN}/{Year}/day/{Day}/answer", content);
        return response.Contains("That's the right answer!");
    }
}
