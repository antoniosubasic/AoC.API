namespace AoC.API.Tests;

/// <summary>
/// The saved response bodies every parser test is pinned to.
/// </summary>
/// <remarks>
/// Each one is a byte-exact copy of what adventofcode.com replied. If the site
/// starts saying something new, save the body here and add the case - rather
/// than loosening a matcher until it guesses.
/// </remarks>
internal static class Fixture
{
    public static string PuzzlePage { get; } = Read("puzzle-day.html");

    public static string Events { get; } = Read("events.html");

    public static string EventsLoggedOut { get; } = Read("events-logged-out.html");

    public static string LoggedOut { get; } = Read("logged-out.html");

    public static string Correct { get; } = Read("submit-correct.html");

    public static string Wrong { get; } = Read("submit-wrong.html");

    public static string TooHigh { get; } = Read("submit-too-high.html");

    public static string TooLow { get; } = Read("submit-too-low.html");

    public static string Cooldown { get; } = Read("submit-cooldown.html");

    public static string AlreadyComplete { get; } = Read("submit-already-complete.html");

    private static string Read(string name) =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
}
