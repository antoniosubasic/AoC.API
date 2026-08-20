using AoC.API.Http;

namespace AoC.API.Tests;

/// <summary>
/// The public API, driven the way a tool built on this package drives it. These
/// run through <see cref="FakeTransport"/>, which is also the point: a tool
/// built on this package can test itself the same way.
/// </summary>
public class ApiTests
{
    private static Puzzle Puzzle => AoC.API.Puzzle.At(2020, 1);

    [Fact]
    public async Task AWholeDayCanBeSolvedWithoutTouchingTheNetwork()
    {
        var transport = new FakeTransport();
        transport
            .PushBody("1721\n979\n366\n299\n675\n1456\n")
            .PushBody(Fixture.TooHigh)
            .PushBody(Fixture.Correct);
        using var session = new Session(transport);

        var input = await session.GetInputLinesAsync(Puzzle);
        Assert.Equal(6, input.Length);

        var rejected = await session.SubmitAnswerAsync(Puzzle, Part.One, "999999");
        Assert.False(rejected.IsCorrect);
        Assert.Equal(Hint.TooHigh, Assert.IsType<Verdict.Incorrect>(rejected).Hint);

        var accepted = await session.SubmitAnswerAsync(Puzzle, Part.One, "514579");
        Assert.IsType<Verdict.Correct>(accepted);

        Assert.Equal(
            [
                "https://adventofcode.com/2020/day/1/input",
                "https://adventofcode.com/2020/day/1/answer",
                "https://adventofcode.com/2020/day/1/answer",
            ],
            transport.RequestedUrls);
    }

    [Fact]
    public async Task ACooldownIsAnExceptionACallerCanBranchOn()
    {
        using var session = new Session(FakeTransport.Serving(Fixture.Cooldown));

        var cooling = await Assert.ThrowsAsync<CooldownException>(
            () => session.SubmitAnswerAsync(Puzzle, Part.Two, "241861950"));

        Assert.Equal(TimeSpan.FromSeconds(270), cooling.Wait);
        Assert.IsAssignableFrom<AdventOfCodeException>(cooling);
    }

    [Fact]
    public async Task AnExpiredCookieIsAnExceptionACallerCanBranchOn()
    {
        var transport = new FakeTransport();
        transport.Push(new TransportResponse(400, Fixture.LoggedOut));
        using var session = new Session(transport);

        await Assert.ThrowsAsync<UnauthorizedException>(() => session.GetInputTextAsync(Puzzle));
    }

    [Fact]
    public void ACoordinateThatIsNotAPuzzleNeverReachesTheTransport()
    {
        var transport = new FakeTransport();
        using var session = new Session(transport);

        Assert.Throws<PuzzleException>(() => AoC.API.Puzzle.At(2025, 25));
        Assert.Throws<PuzzleException>(() => AoC.API.Puzzle.At(2014, 1));
        Assert.Empty(transport.Requests);
        Assert.NotNull(session.Transport);
    }

    [Fact]
    public async Task SamplesAndAcceptedAnswersComeOffThePuzzlePage()
    {
        var transport = new FakeTransport();
        transport.PushBody(Fixture.PuzzlePage).PushBody(Fixture.PuzzlePage);
        using var session = new Session(transport);

        var samples = await session.GetSamplesAsync(Puzzle);
        Assert.Equal(2, samples.Length);
        Assert.StartsWith("1721", samples[0], StringComparison.Ordinal);

        Assert.Equal("241861950", await session.GetAcceptedAnswerAsync(Puzzle, Part.Two));
    }

    [Fact]
    public void ASessionDisposesTheClientItBuiltAndLeavesTheOneItWasGiven()
    {
        var transport = new FakeTransport();

        using (var borrowed = new Session(transport))
        {
            Assert.Same(transport, borrowed.Transport);
        }

        // Disposing a session built around someone else's transport leaves it
        // usable, because the session never owned it.
        Assert.Empty(transport.Requests);
    }
}
