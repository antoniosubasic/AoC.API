using System.Text.RegularExpressions;

namespace AoC.API;

public class APIHandler
{
    private const string DOMAIN = "https://adventofcode.com";

    private string session { get; set; }
    public APIHandler(string session)
    {
        this.session = session;
    }

    private string Request(string url)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Cookie", $"session={session}");

        return client.GetStringAsync(url).Result;
    }


    public string GetInputText(int year, int day) => Request($"{DOMAIN}/{year}/day/{day}/input").TrimEnd('\n');
    public string GetInputText(string input, Regex pattern)
    {
        var match = pattern.Match(input);

        if (match.Success)
        {
            var year = int.Parse(match.Groups["year"].Value);
            var day = int.Parse(match.Groups["day"].Value);

            return GetInputText(year, day);
        }
        else { throw new Exception("no regex match found"); }
    }
    public string[] GetInputLines(int year, int day) => GetInputText(year, day).Split('\n');
    public string[] GetInputLines(string input, Regex pattern) => GetInputText(input, pattern).Split('\n');

    public Dictionary<int, int> GetStars()
    {
        var response = Request($"{DOMAIN}/events")
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
}
