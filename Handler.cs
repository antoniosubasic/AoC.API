namespace AoC.API;

public class APIHandler
{
    const string DOMAIN = "https://adventofcode.com";
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

    public string GetInput(int year, int day) => Request($"{DOMAIN}/{year}/day/{day}/input");

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
