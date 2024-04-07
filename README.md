[![Version](https://img.shields.io/nuget/v/AoCAPI)](https://www.nuget.org/packages/AoCAPI)
[![Downloads](https://img.shields.io/nuget/dt/AoCAPI)](https://www.nuget.org/packages/AoCAPI)

a simple [Advent of Code](https://adventofcode.com) API

## Documentation

- [Add NuGet package](#add-nuget-package)
- [Session initialization](#session-initialization)
- [Features](#features)
    - [Get input](#get-input)
    - [Get sample input](#get-sample-input)
    - [Get achieved stars](#get-achieved-stars)
    - [Submit answer](#submit-answer)

<br><br>

# Add NuGet package

```bash
dotnet add package AoCAPI
```

```csharp
using AoC.API;
```

<br>

# Session initialization

```csharp
var client = new Session("session cookie", int year, int day); // Initializes a new instance of the session class with a specified cookie, year and day.
```

```csharp
var client = new Session("session cookie", string input, Regex pattern); // Initializes a new instance of the session class with a specified cookie, input string and regex pattern.
```

> <picture>
>   <source media="(prefers-color-scheme: dark)" srcset="https://github.com/Mqxx/GitHub-Markdown/blob/main/blockquotes/badge/dark-theme/info.svg">
>   <img alt="Info" src="https://github.com/Mqxx/GitHub-Markdown/blob/main/blockquotes/badge/dark-theme/Info">
> </picture><br>
> The Regex overload needs to have a regex group named "year" and a group named "day".
> <br> <a href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions/Named_capturing_group">How to name Regex groups</a>
> <br> <a href="https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication">How to obtain session cookie</a>

<br>

# Features

## Get input

```csharp
string inputText = await client.GetInputTextAsync(); // Retrieves the input text for the Advent of Code puzzle.
string[] inputLines = await client.GetInputLinesAsync(); // Retrieves the input lines for the Advent of Code puzzle.
```

<br>

## Get sample input

```csharp
string sampleInputText = await client.GetSampleInputTextAsync(int nth); // Retrieves the nth sample input text of the Advent of Code puzzle.
string[] sampleInputLines = await client.GetSampleInputLinesAsync(int nth); // Retrieves the nth sample input lines of the Advent of Code puzzle.
```

<br>

## Get achieved stars

```csharp
Dictionary<int, int> achievedStars = await client.GetAllStarsAsync(); // Retrieves the number of stars earned for each year's Advent of Code. (key: year, value: stars)
```

<br>

## Submit answer

```csharp
Response response = await client.SubmitAnswerAsync(int part, object answer); // Submits the answer for a specific part of the Advent of Code puzzle. Returns whether the answer was true or false and a cooldown if existent.
```

<br><br>

*credits to:*
> [Max](https://github.com/Mqxx) - markdown info icons <br>
> [Monday Morning Haskell](https://mmhaskell.com/) - documentation on how to obtaining session cookie <br>
> [Developer.Mozilla](https://developer.mozilla.org) - documentation on how to name Regex groups
