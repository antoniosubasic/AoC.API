# AGENTS.md

This file provides guidance to coding agents when working with code in this repository.

## What this is

`AoC.API` is a library: a typed client for adventofcode.com that downloads
inputs and samples, reads stars and submits answers. It is published to NuGet as
[`AoCAPI`](https://www.nuget.org/packages/AoCAPI). There is no executable. It
targets `net8.0` and `net10.0`, and it has no package dependencies.

## Commands

```console
$ dotnet build                                  # both target frameworks
$ dotnet test                                   # the whole suite; no network
$ dotnet test --filter FullyQualifiedName~Parsing   # one class, by name
$ dotnet format --verify-no-changes             # style and naming, as CI runs it
$ dotnet pack -c Release                        # the package and its symbols
```

`TreatWarningsAsErrors` is on for every project, so anything that warns locally
fails in CI. That includes analyzer findings, the code-style rules in
`.editorconfig`, and `CS1591` - every public item needs a doc comment.

## Architecture

Four layers, arranged so the fragile part has no I/O and the I/O part has no
parsing:

- **Coordinates** (`Year`, `Day`, `Part`, `Puzzle`) - private constructors,
  `Of`/`At` factories that throw `PuzzleException` and `TryOf`/`TryAt` ones that
  do not. `Puzzle` also owns `BaseUrl` and the three URLs the library talks to.
  The *pairing* is validated, not just each half: events through 2024 run 25
  days, 2025 onwards run 12 (`Year.FirstShort` / `Day.LastShort`). An invalid
  coordinate can therefore never reach the transport. `Part` is an enum, so a
  cast value is caught in `Parts.Number()` before it can be posted as a level.
- **`AoC.API.Http`** - `ITransport` is the seam everything external sits behind,
  plus `TransportRequest`/`TransportResponse` (plain data, no `HttpClient`
  types) and `ClientOptions`. `HttpClientTransport` is the **only** place a
  client is configured and `Configure` is the only place the `User-Agent`
  identification and the session cookie are set. There are deliberately no
  per-call-site headers - that would be a way for a request to go out
  unidentified. `FakeTransport` is a queue of canned replies that also records
  every request; it is public on purpose, because downstream tools test against
  it too.
- **`AoC.API.Parsing`** (internal) - every reply the site sends is a browser
  page, and reading it is the most fragile thing here, so it lives in one
  namespace with no I/O at all: `Html` (`Between`/`AllBetween`/`StripTags`/
  `DecodeEntities`), `Waits` (the site's two ways of writing a duration) and
  `Pages` (samples, accepted answers, stars, submissions). **No regex.**
  `Submission` is what the reply literally said; turning it into a `Verdict` is
  `Session`'s job, because two cases need a second request.
- **`Session`** - the endpoints. It holds an `ITransport` and disposes it only
  when it built it. `Check` decides what a reply means: it asks
  `Pages.IsLoggedOut` **before** looking at the status, because a rejected
  cookie arrives as a `400` from the input endpoint but as an ordinary `200`
  page with a log-in link from the puzzle and events pages.

Two invariants the code is shaped around, both from the Advent of Code
[automation guidelines](https://www.reddit.com/r/adventofcode/wiki/faqs/automation):
**every request carries the caller's identification** (enforced by there being
one place headers are set), and **no request happens that the caller did not
ask for** - nothing polls, retries, prefetches or sleeps. The single exception
is `SubmitAnswerAsync`, which reads the puzzle page when the site says the part
is already solved. Throttling and caching are explicitly the caller's job; do
not add either here. A rejected answer is a `Verdict`, not an exception.

## Working in this codebase

- **Nothing may contact `adventofcode.com`, in tests or in samples.** Test
  against `FakeTransport`. The only tests that open a socket are in
  `HttpTests`, against a loopback `TcpListener` the test stands up itself and
  times out on its own.
- **Parser changes are pinned by fixtures.** Every reply shape has a saved body
  in `tests/AoC.API.Tests/Fixtures` (correct, wrong, too high, too low,
  cooldown, already complete, logged out, a puzzle page, an events page). If the
  site starts saying something new, save the body as a fixture and add the case
  - do not loosen a matcher until it guesses. An unrecognised reply is a
  `ParseException`, deliberately.
- **The fixtures are `-text` in `.gitattributes`** so a Windows checkout cannot
  rewrite their line endings and change what they describe.
- **Match ordinally.** `StringComparison.Ordinal` on every comparison against
  markup; `CA1307` and `CA1310` are errors.
- **Public exceptions live in the root namespace** so a caller needs one
  `using` to catch them, even though the types they describe live deeper.
- Test names are full sentences describing the behaviour
  (`ARejectedAnswerIsAVerdictRatherThanAnException`). Comments explain *why*,
  not what. Messages the library produces are lowercase and phrased the way the
  site phrases things.

## Releases

`PackageVersion` in `src/AoC.API/AoC.API.csproj` is the source of truth. To cut
a release: raise it, write the entry in `CHANGELOG.md`, merge, then push a
`vX.Y.Z` tag matching it. The release workflow refuses to publish if the tag and
the manifest disagree. Nothing publishes on a push to `main`.

Commits follow [Conventional Commits](https://www.conventionalcommits.org), with
`!` marking a breaking change.
