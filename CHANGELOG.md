# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [4.0.0] - 2026-08-20

Version 4 is a rewrite. See [Migrating from 3.x](README.md#migrating-from-3x)
for every removed member and its replacement.

### Added

- `ITransport`, the seam every request goes out through, with `HttpClientTransport` behind it
- `FakeTransport`, which replays canned replies so a tool built on this package can test itself without a network
- `Verdict`, a closed hierarchy describing how the site judged an answer
- `AdventOfCodeException` and the typed exceptions under it: `TransportException`, `ParseException`, `UnauthorizedException`, `PuzzleLockedException`, `CooldownException` and `UnexpectedStatusException`
- `Puzzle`, `Year`, `Day` and `Part`, which validate a coordinate - and the pairing of a year with a day - before it can become a request
- `CancellationToken` support on every call
- `ClientOptions`, which carries the identification and an adjustable timeout
- `session.GetSamplesAsync` and `session.GetAcceptedAnswerAsync`
- .NET 8 support alongside .NET 10; the package ships XML documentation and a symbol package

### Changed

- **Breaking:** every call takes the puzzle as an argument, so one session serves a whole event
- **Breaking:** callers must provide their own identification, which every request then carries as its `User-Agent`
- **Breaking:** `SubmitAnswerAsync` returns a `Verdict` instead of a `SubmissionResult`, and a cooldown that judged nothing is a `CooldownException`
- **Breaking:** `GetAllStarsAsync` becomes `GetStarsAsync` and returns `IReadOnlyDictionary<Year, int>`, earliest event first
- **Breaking:** `GetSampleInputTextAsync` and `GetSampleInputLinesAsync` become `GetSampleTextAsync` and `GetSampleLinesAsync`
- a cooldown is a `TimeSpan` rather than the site's own prose, in both shapes the site writes it
- the site's replies are read by a parser of its own, pinned by saved response bodies, rather than by regular expressions

### Fixed

- one `HttpClient` is built per session rather than one per request, which used to leak sockets
- an expired cookie is reported as `UnauthorizedException` even on the pages that answer `200` with a log-in link
- an answer the site never asked for - part two while part one is open, or day 25's second star - is `Verdict.WrongLevel` rather than a failure to read the reply
- the currently running event is counted in the star totals

### Removed

- **Breaking:** the `Regex` constructor overload; recover the year and day yourself and pass `Puzzle.At(year, day)`
- **Breaking:** `SubmissionResult`, `SubmissionStatus`, `RegexMatchException` and `UnknownResponseException`
- the `System.Text.RegularExpressions` package reference; this package has no dependencies

[Unreleased]: https://github.com/antoniosubasic/AoC.API/compare/v4.0.0...HEAD
[4.0.0]: https://github.com/antoniosubasic/AoC.API/compare/v3.0.0...v4.0.0
