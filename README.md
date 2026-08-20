# AoC.API

[![CI](https://github.com/antoniosubasic/AoC.API/actions/workflows/ci.yml/badge.svg)](https://github.com/antoniosubasic/AoC.API/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/AoCAPI)](https://www.nuget.org/packages/AoCAPI)
[![Downloads](https://img.shields.io/nuget/dt/AoCAPI)](https://www.nuget.org/packages/AoCAPI)
[![License](https://img.shields.io/nuget/l/AoCAPI)](https://github.com/antoniosubasic/AoC.API/blob/main/LICENSE)

A typed client for [Advent of Code](https://adventofcode.com): downloads puzzle
inputs and samples, reads how many stars an account has earned, and submits
answers — identifying itself as the
[automation guidelines](https://www.reddit.com/r/adventofcode/wiki/faqs/automation)
ask. There is also a [Rust version](https://github.com/antoniosubasic/aoc_api).

```console
$ dotnet add package AoCAPI
```

```csharp
using AoC.API;

using var session = new Session("53616c7465645f5f...", "github.com/my-username/my-repo by me@example.com");
var puzzle = Puzzle.At(2024, 7);

var input = await session.GetInputTextAsync(puzzle);

Console.WriteLine(await session.SubmitAnswerAsync(puzzle, Part.One, "3749") switch
{
    Verdict.Correct => "gold star",
    var verdict => verdict.ToString(),
});
```

The cookie is the value of the `session` cookie on `adventofcode.com` while
logged in ([how to find it](https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication)).
It is a credential: treat it like a password. This package keeps it out of
`ToString` output and never logs it.

Targets .NET 8 and .NET 10.

## What it does

| Call | Returns |
| --- | --- |
| `session.GetInputTextAsync(puzzle)` | the puzzle's personal input, without its trailing newline |
| `session.GetInputLinesAsync(puzzle)` | the same, as `string[]` |
| `session.GetSamplesAsync(puzzle)` | every sample block on the puzzle page |
| `session.GetSampleTextAsync(puzzle, nth)` | the `nth` sample block, counting from one |
| `session.GetSampleLinesAsync(puzzle, nth)` | the same, as `string[]` |
| `session.GetStarsAsync()` | `IReadOnlyDictionary<Year, int>` — stars earned per event, earliest first |
| `session.GetAcceptedAnswerAsync(puzzle, part)` | the answer the puzzle page shows as accepted |
| `session.SubmitAnswerAsync(puzzle, part, answer)` | a [`Verdict`](#verdicts-and-exceptions) |

One session serves a whole event: it holds the cookie and the single
`HttpClient` built from it, and which puzzle a call is about is an argument.
Every call takes a `CancellationToken`. Build the session outside your loop —
it is what carries your identification.

### With a client of your own

`Session` also takes an `ITransport`, which is the seam every request goes out
through. `HttpClientTransport` is the real one, and it can be handed a client
you own — from an `IHttpClientFactory`, say:

```csharp
using AoC.API.Http;

var options = new ClientOptions(cookie, "github.com/my-username/my-repo by me@example.com")
{
    Timeout = TimeSpan.FromSeconds(10),
};

var transport = new HttpClientTransport(httpClientFactory.CreateClient("adventofcode"), options);
using var session = new Session(transport);
```

Give it a client of its own: the identification and the session cookie are that
client's default headers, so no request can leave without them and no other
caller should be sharing it.

### Validated coordinates

`Puzzle`, `Year`, `Day` and `Part` are validated, so an out-of-range coordinate
cannot become a request. The *pairing* is validated too — events up to 2024 run
25 puzzles, and from 2025 on they run 12:

```csharp
Puzzle.At(2024, 25); // fine
Puzzle.At(2025, 25); // PuzzleException: the 2025 event stops after day 12
Puzzle.At(1066, 1);  // PuzzleException: advent of code started in 2015

Puzzle.TryAt(2025, 25, out var puzzle); // false, for a coordinate you did not write yourself
```

### Verdicts and exceptions

A rejected answer is a `Verdict`, not an exception — being wrong is a normal
outcome. The hierarchy is closed, so a `switch` over it can be exhaustive:

| `Verdict` | Meaning |
| --- | --- |
| `Correct` | accepted |
| `Incorrect(Hint?, TimeSpan?)` | rejected; the hint is `TooHigh`/`TooLow` when the site says so, the wait is how long it asks you to wait |
| `AlreadyComplete(bool Matches)` | the part was already solved, so the site refused to judge; the answer was compared against the accepted one on the puzzle page instead |
| `WrongLevel` | the site was not asking for an answer to that part — either part one is still unsolved, or the part was never a question, which is day 25's second star |

`verdict.IsCorrect` collapses that to a `bool` when that is all you need.

Everything that stops a call from producing an answer is an
`AdventOfCodeException` you can catch as one, or branch on:
`TransportException` (the request failed), `UnauthorizedException` (the cookie
is missing, expired or invalid), `PuzzleLockedException` (the puzzle has not
unlocked yet), `CooldownException` (an answer was submitted too recently, so
nothing was judged — it carries the remaining `Wait`), `ParseException` (the
reply was not one this package recognises) and `UnexpectedStatusException`
(anything else the site returned). Coordinates that name no puzzle are a
`PuzzleException`, which is an `ArgumentException`: nothing was asked of the
site, and nothing will be.

```csharp
try
{
    var verdict = await session.SubmitAnswerAsync(puzzle, Part.One, answer);
}
catch (CooldownException cooling)
{
    await Task.Delay(cooling.Wait);
}
```

### Testing without a network

`ITransport` is the seam everything external sits behind, and `FakeTransport`
replays canned replies. This package's own tests run entirely through it — no
network, no session cookie — and it ships so a tool built on this package can
do the same:

```csharp
using AoC.API.Http;

var transport = FakeTransport.Serving("1721\n979\n366\n");
using var session = new Session(transport);

var input = await session.GetInputTextAsync(Puzzle.At(2020, 1));

Assert.Equal("1721\n979\n366", input);
Assert.Equal(["https://adventofcode.com/2020/day/1/input"], transport.RequestedUrls);
```

## Automation etiquette

This package follows the Advent of Code
[automation guidelines](https://www.reddit.com/r/adventofcode/wiki/faqs/automation).
Two of them are settled here; two are deliberately left to you, and this
section says which is which so you can be accurate about your own tool.

- **Every request identifies you.** You provide your identification when the
  session is opened, and it becomes one of the HTTP client's default headers,
  so no call site can omit it and no endpoint can add one of its own. The
  guidelines ask for `github.com/your-repo by you@example.com` or similar.
  Since this is a library, the tool built upon it is the one doing the work, so
  it is the one that must identify itself.
- **Nothing happens that you did not ask for.** A request is made when you call
  an endpoint and at no other time: nothing polls, retries, prefetches or runs
  on a schedule. The one call that can make two requests is
  `SubmitAnswerAsync`, and only when the site says the part is already solved,
  in which case it reads the puzzle page to compare your answer against the
  accepted one.
- **Throttling is yours.** This package does not sleep between requests. A
  library cannot know how a program is being driven, and a hidden delay inside
  someone else's process is a poor surprise — a caller that already paces
  itself would end up paying twice. Space your calls out; five seconds between
  them is a sensible floor.
- **Caching is yours.** Puzzle inputs are personal, permanent and unchanging,
  so download one once and keep it on disk. This package hands you the body and
  forgets it; it never re-downloads on its own, and it never downloads anything
  you did not ask for.

Please do not work around the first two.

## Design notes

Decisions made during the rewrite, and why:

- **Everything external sits behind `ITransport`.** The endpoints and the
  parser never learn how a reply was obtained, which is what lets the whole
  suite run with no network and no cookie — and lets a tool built on this
  package test itself the same way.
- **No regular expressions.** The site's replies are read by a small
  dependency-free parser in [`src/AoC.API/Parsing`](src/AoC.API/Parsing),
  pinned by saved response bodies for every shape the site sends: correct,
  wrong, too high, too low, cooldown, already complete and logged out. It also
  drops emphasis markup and decodes entities, so a sample comes back as the
  puzzle shows it rather than as page source. The 3.x patterns failed
  unreadably whenever a wording changed.
- **A cooldown is a `TimeSpan`.** The remaining wait used to be handed back as
  the site's own prose. It is now parsed, in both shapes the site uses
  (`4m 30s` and `one minute`), so a caller can actually wait for it.
- **One client, built once.** 3.x built an `HttpClient` per request, which is
  the classic way to exhaust sockets. There is now exactly one, built where the
  session is opened, and it is the only place the identification and the cookie
  are set.
- **A wrong answer is not an exception.** 3.x threw for replies it did not
  recognise and returned `false` for ones it did, which put "you were wrong"
  and "the site changed" in the same shape. They are now a `Verdict` and a
  `ParseException` respectively.

## Migrating from 3.x

Version 4 is a rewrite. Every removed member and its replacement:

| 3.x | 4.x |
| --- | --- |
| `new Session(cookie, year, day)` | `new Session(cookie, "identification")` plus `Puzzle.At(year, day)` |
| `new Session(cookie, input, pattern)` | recover the year and day yourself, then `Puzzle.At(year, day)` |
| `session.GetInputTextAsync()` | `session.GetInputTextAsync(puzzle)` |
| `session.GetInputLinesAsync()` | `session.GetInputLinesAsync(puzzle)` |
| `session.GetSampleInputTextAsync(nth)` | `session.GetSampleTextAsync(puzzle, nth)` |
| `session.GetSampleInputLinesAsync(nth)` | `session.GetSampleLinesAsync(puzzle, nth)` |
| `session.GetAllStarsAsync()` → `Dictionary<int, int>` | `session.GetStarsAsync()` → `IReadOnlyDictionary<Year, int>` |
| `session.SubmitAnswerAsync(part, answer)` → `SubmissionResult` | `session.SubmitAnswerAsync(puzzle, part, answer)` → `Verdict` |
| `SubmissionResult.IsCorrect` | `verdict.IsCorrect` |
| `SubmissionResult.CooldownTime` *(a `string`)* | `Verdict.Incorrect.Wait`, or `CooldownException.Wait` *(a `TimeSpan`)* |
| `SubmissionStatus.OnCooldown` | `CooldownException` — nothing was judged, so it is not a verdict |
| `SubmissionResult` / `SubmissionStatus` | `Verdict` says what happened |
| `RegexMatchException`, `UnknownResponseException` | `ParseException` |
| *(reported as one of the above)* | `UnauthorizedException`, `PuzzleLockedException`, `UnexpectedStatusException` |

A whole call site, before and after:

```csharp
// 3.x
var session = new Session(cookie, 2024, 7);
var input = await session.GetInputTextAsync();
var result = await session.SubmitAnswerAsync(1, "3749");
if (result.Status == SubmissionStatus.OnCooldown) { Console.WriteLine($"wait {result.CooldownTime}"); }
else { Console.WriteLine(result.IsCorrect ? "correct" : "wrong"); }

// 4.x
using var session = new Session(cookie, "github.com/my-username/my-repo by me@example.com");
var puzzle = Puzzle.At(2024, 7);
var input = await session.GetInputTextAsync(puzzle);

try
{
    Console.WriteLine((await session.SubmitAnswerAsync(puzzle, Part.One, "3749")).ToString());
}
catch (CooldownException cooling)
{
    Console.WriteLine($"wait {cooling.Wait}");
}
```

Note that a rejected answer the site asked you to wait after used to arrive as
a cooldown, losing the fact that it had been judged at all. It is now
`Verdict.Incorrect` with a `Wait`, and `CooldownException` means only what the
site means by it: nothing was judged, so the answer still has to be submitted
again.

## Development

```console
$ dotnet build                            # net8.0 and net10.0; warnings are errors
$ dotnet test                             # the whole suite; no network
$ dotnet format --verify-no-changes       # style and naming, as CI runs it
$ dotnet pack -c Release                  # the package and its symbols
```

No test contacts `adventofcode.com`: endpoints and parsing run through
`FakeTransport` and saved response bodies, and the handful of tests that
exercise the real client talk to a loopback listener the test itself stands up.
Running the suite on .NET 8 as well as .NET 10 needs both runtimes installed;
with only the newer one it rolls forward.

Releases are cut by pushing a `vX.Y.Z` tag that matches `PackageVersion` in
[`src/AoC.API/AoC.API.csproj`](src/AoC.API/AoC.API.csproj); the release
workflow packs, publishes to NuGet and opens a GitHub release.

## License

[GPL-3.0](LICENSE)
