using AoC.API.Http;

namespace AoC.API.Tests;

public class SessionTests
{
    private static Puzzle Puzzle => AoC.API.Puzzle.At(2020, 1);

    [Fact]
    public async Task AnInputIsFetchedFromTheInputEndpointWithoutItsTrailingNewline()
    {
        var transport = FakeTransport.Serving("1721\n979\n366\n");
        using var session = new Session(transport);

        var input = await session.GetInputTextAsync(Puzzle);

        Assert.Equal("1721\n979\n366", input);
        Assert.Equal(["https://adventofcode.com/2020/day/1/input"], transport.RequestedUrls);
    }

    [Fact]
    public async Task AnInputCanBeReadAsLines()
    {
        using var session = new Session(FakeTransport.Serving("1721\n979\n366\n"));

        Assert.Equal(["1721", "979", "366"], await session.GetInputLinesAsync(Puzzle));
    }

    [Fact]
    public async Task SamplesComeFromThePuzzlePage()
    {
        var transport = FakeTransport.Serving(Fixture.PuzzlePage);
        using var session = new Session(transport);

        var sample = await session.GetSampleLinesAsync(Puzzle, 1);

        Assert.Equal(["1721", "979", "366", "299", "675", "1456"], sample);
        Assert.Equal(["https://adventofcode.com/2020/day/1"], transport.RequestedUrls);
    }

    [Fact]
    public async Task EverySampleOnThePageCanBeReadAtOnce()
    {
        using var session = new Session(FakeTransport.Serving(Fixture.PuzzlePage));

        Assert.Equal(2, (await session.GetSamplesAsync(Puzzle)).Length);
    }

    [Fact]
    public async Task ASampleZeroNeverReachesTheTransport()
    {
        var transport = new FakeTransport();
        using var session = new Session(transport);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => session.GetSampleTextAsync(Puzzle, 0));

        Assert.Empty(transport.Requests);
    }

    [Fact]
    public async Task StarsAreReadFromTheEventsPage()
    {
        var transport = FakeTransport.Serving(Fixture.Events);
        using var session = new Session(transport);

        var stars = await session.GetStarsAsync();

        Assert.Equal([0, 9, 50, 19], stars.Values);
        Assert.Equal(["https://adventofcode.com/events"], transport.RequestedUrls);
    }

    [Fact]
    public async Task AnAcceptedAnswerIsPostedToTheAnswerEndpoint()
    {
        var transport = FakeTransport.Serving(Fixture.Correct);
        using var session = new Session(transport);

        var verdict = await session.SubmitAnswerAsync(Puzzle, Part.Two, "241861950");

        Assert.IsType<Verdict.Correct>(verdict);
        Assert.True(verdict.IsCorrect);

        var request = Assert.Single(transport.Requests);
        Assert.Equal(RequestMethod.Post, request.Method);
        Assert.Equal("https://adventofcode.com/2020/day/1/answer", request.Url);
        Assert.Equal(
            [new KeyValuePair<string, string>("level", "2"), new KeyValuePair<string, string>("answer", "241861950")],
            request.Form);
    }

    [Fact]
    public async Task ARejectedAnswerIsAVerdictRatherThanAnException()
    {
        using var session = new Session(FakeTransport.Serving(Fixture.Wrong));

        var verdict = Assert.IsType<Verdict.Incorrect>(await session.SubmitAnswerAsync(Puzzle, Part.One, "0"));

        Assert.Null(verdict.Hint);
        Assert.Equal(TimeSpan.FromSeconds(60), verdict.Wait);
        Assert.False(verdict.IsCorrect);
    }

    [Fact]
    public async Task ASubmissionOnCooldownReportsTheRemainingWait()
    {
        using var session = new Session(FakeTransport.Serving(Fixture.Cooldown));

        var cooling = await Assert.ThrowsAsync<CooldownException>(
            () => session.SubmitAnswerAsync(Puzzle, Part.One, "514579"));

        Assert.Equal(TimeSpan.FromSeconds(270), cooling.Wait);
        Assert.Contains("4m 30s left to wait", cooling.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnAnswerToASolvedPartIsCheckedAgainstThePuzzlePage()
    {
        var transport = new FakeTransport();
        transport.PushBody(Fixture.AlreadyComplete).PushBody(Fixture.PuzzlePage);
        using var session = new Session(transport);

        var verdict = Assert.IsType<Verdict.AlreadyComplete>(
            await session.SubmitAnswerAsync(Puzzle, Part.One, "514579"));

        Assert.True(verdict.Matches);
        Assert.True(verdict.IsCorrect);
        Assert.Equal(
            ["https://adventofcode.com/2020/day/1/answer", "https://adventofcode.com/2020/day/1"],
            transport.RequestedUrls);
    }

    [Fact]
    public async Task ADifferentAnswerToASolvedPartIsStillWrong()
    {
        var transport = new FakeTransport();
        transport.PushBody(Fixture.AlreadyComplete).PushBody(Fixture.PuzzlePage);
        using var session = new Session(transport);

        var verdict = Assert.IsType<Verdict.AlreadyComplete>(await session.SubmitAnswerAsync(Puzzle, Part.One, "1"));

        Assert.False(verdict.Matches);
        Assert.False(verdict.IsCorrect);
    }

    // Day 25 asks one question and gives its second star away, and part two of
    // any day refuses answers while part one is open. The site says "you don't
    // seem to be solving the right level" to both, and its page has no accepted
    // answer to compare against either way - which is a verdict about the
    // question, not a failure to read the reply.
    [Theory]
    [InlineData("<p>Your puzzle answer was <code>514579</code>.</p>")]
    [InlineData("<p>To begin, please identify yourself.</p>")]
    public async Task AnAnswerTheSiteNeverAskedForIsAVerdictRatherThanAParseError(string page)
    {
        var transport = new FakeTransport();
        transport.PushBody(Fixture.AlreadyComplete).PushBody(page);
        using var session = new Session(transport);

        var verdict = await session.SubmitAnswerAsync(Puzzle, Part.Two, "241861950");

        Assert.IsType<Verdict.WrongLevel>(verdict);
        Assert.False(verdict.IsCorrect);
    }

    [Fact]
    public async Task AnAcceptedAnswerCanBeReadOffThePuzzlePage()
    {
        using var session = new Session(FakeTransport.Serving(Fixture.PuzzlePage));

        Assert.Equal("241861950", await session.GetAcceptedAnswerAsync(Puzzle, Part.Two));
    }

    [Fact]
    public async Task APartWithNoAcceptedAnswerSaysWhichPartIsMissing()
    {
        using var session = new Session(FakeTransport.Serving("<p>nothing solved here</p>"));

        var missing = await Assert.ThrowsAsync<ParseException>(
            () => session.GetAcceptedAnswerAsync(Puzzle, Part.One));

        Assert.Contains("part 1", missing.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AReplyAskingForALoginIsAnExpiredCookieWhateverItsStatus()
    {
        var transport = new FakeTransport();
        transport.Push(new TransportResponse(400, Fixture.LoggedOut));
        using var session = new Session(transport);

        await Assert.ThrowsAsync<UnauthorizedException>(() => session.GetInputTextAsync(Puzzle));
    }

    [Fact]
    public async Task APageOfferingALoginIsAnExpiredCookieEvenAtStatus200()
    {
        using var session = new Session(FakeTransport.Serving(Fixture.EventsLoggedOut));

        await Assert.ThrowsAsync<UnauthorizedException>(() => session.GetStarsAsync());
    }

    [Fact]
    public async Task APuzzleThatHasNotUnlockedSaysSo()
    {
        var transport = new FakeTransport();
        transport.Push(new TransportResponse(404, "Please don't repeatedly request this endpoint before it unlocks!"));
        using var session = new Session(transport);

        var locked = await Assert.ThrowsAsync<PuzzleLockedException>(() => session.GetInputTextAsync(Puzzle));

        Assert.Equal(Puzzle, locked.Puzzle);
    }

    [Fact]
    public async Task AnUnexpectedStatusIsReportedAsItself()
    {
        var transport = new FakeTransport();
        transport.Push(new TransportResponse(500, "<h1>Internal Server Error</h1>"));
        using var session = new Session(transport);

        var unwell = await Assert.ThrowsAsync<UnexpectedStatusException>(() => session.GetStarsAsync());

        Assert.Equal(500, unwell.StatusCode);
    }

    [Fact]
    public async Task AReplyTheParserDoesNotKnowIsAnErrorRatherThanAGuess()
    {
        using var session = new Session(FakeTransport.Serving("<p>Ho ho ho.</p>"));

        await Assert.ThrowsAsync<ParseException>(() => session.SubmitAnswerAsync(Puzzle, Part.One, "514579"));
    }

    [Fact]
    public async Task ATransportFailureIsReportedRatherThanRetried()
    {
        var transport = new FakeTransport();
        using var session = new Session(transport);

        await Assert.ThrowsAsync<TransportException>(() => session.GetInputTextAsync(Puzzle));

        Assert.Single(transport.Requests);
    }

    [Fact]
    public async Task ACancelledCallDoesNotReachTheTransport()
    {
        var transport = new FakeTransport();
        using var session = new Session(transport);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => session.GetInputTextAsync(Puzzle, cancellation.Token));

        Assert.Empty(transport.Requests);
    }

    [Fact]
    public void AVerdictReadsLikeTheSitePhrasesIt()
    {
        Assert.Equal("that's the right answer", new Verdict.Correct().ToString());
        Assert.Equal(
            "that's not the right answer; it is too high (wait 1m before trying again)",
            new Verdict.Incorrect(Hint.TooHigh, TimeSpan.FromSeconds(60)).ToString());
        Assert.Equal("that's not the right answer", new Verdict.Incorrect(null, null).ToString());
        Assert.Equal("already solved, with this answer", new Verdict.AlreadyComplete(Matches: true).ToString());
        Assert.Equal("already solved, with a different answer", new Verdict.AlreadyComplete(Matches: false).ToString());
        Assert.Equal("there is nothing to answer for this part", new Verdict.WrongLevel().ToString());
    }
}
