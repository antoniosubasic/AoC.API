namespace AoC.API.Tests;

public class PuzzleTests
{
    [Fact]
    public void AYearBeforeTheFirstEventIsNotAYear()
    {
        Assert.Throws<PuzzleException>(() => Year.Of(2014));
        Assert.Equal(2015, Year.Of(2015).Value);
        Assert.Equal(2024, Year.Of(2024).Value);
    }

    [Fact]
    public void OnlyPuzzleDaysAreDays()
    {
        Assert.Throws<PuzzleException>(() => Day.Of(0));
        Assert.Throws<PuzzleException>(() => Day.Of(26));
        Assert.Equal(1, Day.Of(1).Value);
        Assert.Equal(25, Day.Of(25).Value);
    }

    [Fact]
    public void EventsFrom2025OnAreHalfAsLong()
    {
        Assert.Equal(25, Year.Of(2015).LastDay.Value);
        Assert.Equal(25, Year.Of(2024).LastDay.Value);
        Assert.Equal(12, Year.Of(2025).LastDay.Value);
        Assert.Equal(12, Year.Of(2030).LastDay.Value);

        Assert.True(Year.Of(2024).HasDay(Day.Of(25)));
        Assert.True(Year.Of(2025).HasDay(Day.Of(12)));
        Assert.False(Year.Of(2025).HasDay(Day.Of(13)));
    }

    [Fact]
    public void ADayItsEventNeverPublishedIsNotAPuzzle()
    {
        Assert.NotNull(Puzzle.Of(Year.Of(2024), Day.Of(25)));
        Assert.NotNull(Puzzle.Of(Year.Of(2025), Day.Of(12)));

        var rejected = Assert.Throws<PuzzleException>(() => Puzzle.Of(Year.Of(2025), Day.Of(25)));
        Assert.Contains("the 2025 event stops after day 12", rejected.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RawNumbersAreValidatedBeforeTheyCanBecomeARequest()
    {
        Assert.NotNull(Puzzle.At(2024, 7));

        Assert.Contains("no 1066 event", Assert.Throws<PuzzleException>(() => Puzzle.At(1066, 7)).Message, StringComparison.Ordinal);
        Assert.Contains("days 1 to 25", Assert.Throws<PuzzleException>(() => Puzzle.At(2024, 47)).Message, StringComparison.Ordinal);
        Assert.Throws<PuzzleException>(() => Puzzle.At(2025, 20));
    }

    [Fact]
    public void CoordinatesCanBeCheckedWithoutAnException()
    {
        Assert.True(Puzzle.TryAt(2024, 7, out var puzzle));
        Assert.Equal(Puzzle.At(2024, 7), puzzle);

        Assert.False(Puzzle.TryAt(2025, 25, out var missing));
        Assert.Null(missing);
        Assert.False(Puzzle.TryAt(1066, 1, out _));
        Assert.False(Puzzle.TryAt(2024, 0, out _));
    }

    [Fact]
    public void APuzzleKnowsItsThreeUrls()
    {
        var puzzle = Puzzle.At(2024, 5);

        Assert.Equal("https://adventofcode.com/2024/day/5", puzzle.Url);
        Assert.Equal("https://adventofcode.com/2024/day/5/input", puzzle.InputUrl);
        Assert.Equal("https://adventofcode.com/2024/day/5/answer", puzzle.AnswerUrl);
    }

    [Fact]
    public void APuzzleNamesItself()
    {
        Assert.Equal("2024 day 5", Puzzle.At(2024, 5).ToString());
        Assert.Equal("2024", Year.Of(2024).ToString());
        Assert.Equal("5", Day.Of(5).ToString());
    }

    [Fact]
    public void PuzzlesCompareByYearThenDay()
    {
        Assert.True(Puzzle.At(2020, 1) < Puzzle.At(2020, 2));
        Assert.True(Puzzle.At(2021, 1) > Puzzle.At(2020, 25));
        Assert.True(Year.Of(2020) < Year.Of(2021));
        Assert.True(Day.Of(25) >= Day.Of(25));
        Assert.Equal(Puzzle.At(2020, 1), Puzzle.At(2020, 1));
    }

    [Fact]
    public void PartsMapToApiNumbers()
    {
        Assert.Equal(1, Part.One.Number());
        Assert.Equal(2, Part.Two.Number());
        Assert.Equal(0, Part.One.Index());
        Assert.Equal(1, Part.Two.Index());
        Assert.Equal("part 2", Part.Two.Describe());

        Assert.Equal(Part.One, Parts.FromNumber(1));
        Assert.Equal(Part.Two, Parts.FromNumber(2));
        Assert.Throws<PuzzleException>(() => Parts.FromNumber(3));
        Assert.False(Parts.TryFromNumber(0, out _));
    }

    [Fact]
    public void APartCastFromANumberTheEnumNeverDeclaredIsRejected()
    {
        // An enum will hold anything of its underlying type, so a cast is the
        // one way an invalid part can turn up - and it is caught before it can
        // be posted as a level the site has no idea what to do with.
        Assert.Throws<PuzzleException>(() => ((Part)7).Number());
    }
}
