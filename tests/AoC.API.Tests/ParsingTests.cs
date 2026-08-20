using AoC.API.Parsing;

namespace AoC.API.Tests;

public class ParsingTests
{
    [Fact]
    public void AnAcceptedAnswerIsRecognised() =>
        Assert.IsType<Submission.Correct>(Pages.ReadSubmission(Fixture.Correct));

    [Fact]
    public void ARejectedAnswerCarriesTheDirectionItWasWrongIn()
    {
        var high = Assert.IsType<Submission.Incorrect>(Pages.ReadSubmission(Fixture.TooHigh));
        Assert.Equal(Hint.TooHigh, high.Hint);
        Assert.Equal(TimeSpan.FromSeconds(60), high.Wait);

        var low = Assert.IsType<Submission.Incorrect>(Pages.ReadSubmission(Fixture.TooLow));
        Assert.Equal(Hint.TooLow, low.Hint);
        Assert.Equal(TimeSpan.FromSeconds(300), low.Wait);
    }

    [Fact]
    public void ARejectedAnswerWithoutAHintIsStillARejection()
    {
        var wrong = Assert.IsType<Submission.Incorrect>(Pages.ReadSubmission(Fixture.Wrong));

        Assert.Null(wrong.Hint);
        Assert.Equal(TimeSpan.FromSeconds(60), wrong.Wait);
    }

    [Fact]
    public void ASubmissionOnCooldownReportsTheRemainingWait()
    {
        var cooling = Assert.IsType<Submission.TooRecent>(Pages.ReadSubmission(Fixture.Cooldown));

        Assert.Equal(TimeSpan.FromSeconds((4 * 60) + 30), cooling.Wait);
    }

    [Fact]
    public void APartThatIsAlreadySolvedIsRecognised() =>
        Assert.IsType<Submission.AlreadyComplete>(Pages.ReadSubmission(Fixture.AlreadyComplete));

    [Fact]
    public void AReplyThatAsksForALoginIsRecognised()
    {
        Assert.IsType<Submission.LoggedOut>(Pages.ReadSubmission(Fixture.LoggedOut));
        Assert.True(Pages.IsLoggedOut(Fixture.LoggedOut));
        Assert.True(Pages.IsLoggedOut(Fixture.EventsLoggedOut));
        Assert.False(Pages.IsLoggedOut(Fixture.PuzzlePage));
    }

    [Fact]
    public void AnUnrecognisedReplyReportsWhatTheSiteSaid()
    {
        var unreadable = Assert.Throws<ParseException>(
            () => Pages.ReadSubmission("<main><article><p>Ho ho ho.</p></article></main>"));

        Assert.Contains("Ho ho ho.", unreadable.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnrecognisedReplyDoesNotRepeatAWholePageBack()
    {
        var page = $"<p>{string.Concat(Enumerable.Repeat("lorem ipsum ", 100))}</p>";

        var unreadable = Assert.Throws<ParseException>(() => Pages.ReadSubmission(page));

        Assert.EndsWith("...", unreadable.Message, StringComparison.Ordinal);
        Assert.True(unreadable.Message.Length < 300, unreadable.Message);
    }

    [Fact]
    public void SamplesAreReadWithoutTheirMarkup()
    {
        var samples = Pages.Samples(Fixture.PuzzlePage);

        Assert.Equal(2, samples.Length);
        Assert.Equal("1721\n979\n366\n299\n675\n1456", samples[0]);
        Assert.Equal("1721\n979\n366\n299\n675\n1456", Pages.Sample(Fixture.PuzzlePage, 1));
    }

    [Fact]
    public void EmphasisAndEntitiesInsideASampleAreResolved() =>
        Assert.Equal("3 & 4 <target>\n7 > 5", Pages.Sample(Fixture.PuzzlePage, 2));

    [Fact]
    public void AskingForASampleAPageDoesNotHaveSaysHowManyThereAre()
    {
        var missing = Assert.Throws<ParseException>(() => Pages.Sample(Fixture.PuzzlePage, 3));

        Assert.Contains("has 2 sample block(s)", missing.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThereIsNoSampleZeroBecauseTheyCountFromOne() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Pages.Sample(Fixture.PuzzlePage, 0));

    [Fact]
    public void ASolvedPageShowsBothAcceptedAnswers()
    {
        Assert.Equal(["514579", "241861950"], Pages.AcceptedAnswers(Fixture.PuzzlePage));

        Assert.True(Pages.TryAcceptedAnswer(Fixture.PuzzlePage, Part.One, out var one));
        Assert.Equal("514579", one);
        Assert.True(Pages.TryAcceptedAnswer(Fixture.PuzzlePage, Part.Two, out var two));
        Assert.Equal("241861950", two);
    }

    [Fact]
    public void APageWithoutAnAcceptedAnswerHasNoneToGive()
    {
        Assert.False(Pages.TryAcceptedAnswer("<p>nothing solved here</p>", Part.One, out var answer));
        Assert.Null(answer);
    }

    [Fact]
    public void TheEventsPageListsEveryEventAndItsStars()
    {
        var stars = Pages.Stars(Fixture.Events);

        Assert.Equal(4, stars.Count);
        Assert.Equal(50, stars[Year.Of(2024)]);
        Assert.Equal(9, stars[Year.Of(2023)]);
        Assert.Equal(0, stars[Year.Of(2022)]);
    }

    [Fact]
    public void TheRunningEventCountsEvenThoughItLinksToTheFrontPage()
    {
        var stars = Pages.Stars(Fixture.Events);

        Assert.Equal(19, stars[Year.Of(2025)]);
        Assert.Equal(78, stars.Values.Sum());
    }

    [Fact]
    public void StarsComeBackEarliestEventFirst() =>
        Assert.Equal([2022, 2023, 2024, 2025], Pages.Stars(Fixture.Events).Keys.Select(year => year.Value));

    [Fact]
    public void AnEventsPageWithNoEventsIsAnErrorRatherThanAnEmptyMap() =>
        Assert.Throws<ParseException>(() => Pages.Stars("<main><p>nothing here</p></main>"));

    [Theory]
    [InlineData("4m 30s", 270)]
    [InlineData("30s", 30)]
    [InlineData("1h 2m 3s", 3723)]
    [InlineData("one minute", 60)]
    [InlineData("5 minutes", 300)]
    [InlineData("2 hours", 7200)]
    [InlineData("ten seconds", 10)]
    public void WaitsAreReadInBothOfTheShapesTheSiteUses(string written, int seconds)
    {
        Assert.True(Waits.TryParse(written, out var wait));
        Assert.Equal(TimeSpan.FromSeconds(seconds), wait);
    }

    [Fact]
    public void SomethingThatIsNotAWaitIsNotReadAsOne() => Assert.False(Waits.TryParse("shortly", out _));

    [Theory]
    [InlineData(270, "4m 30s")]
    [InlineData(60, "1m")]
    [InlineData(0, "0s")]
    [InlineData(3723, "1h 2m 3s")]
    public void WaitsAreRenderedTheWayTheSitePhrasesThem(int seconds, string written) =>
        Assert.Equal(written, Waits.Describe(TimeSpan.FromSeconds(seconds)));
}
