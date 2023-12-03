[![Version](https://img.shields.io/nuget/v/AoCAPI)](https://www.nuget.org/packages/AoCAPI)
[![Downloads](https://img.shields.io/nuget/dt/AoCAPI)](https://www.nuget.org/packages/AoCAPI)

a simple [Advent of Code](https://adventofcode.com) API

## Documentation

- [Add NuGet package](#add-nuget-package)
- [Session initialization](#session-initialization)
- [Features](#features)
    - [Get input file](#get-input-file)
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
var client = new Session("session cookie", int year, int day);
```

```csharp
var client = new Session("session cookie", string input, Regex pattern);
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

## Get input file

```csharp
string inputText = client.GetInputText(); // input file (raw text)
string inputLines = client.GetInputLines(); // input file (lines array)
```

<br>

## Get achieved stars

```csharp
Dictionary<int, int> achievedStars = client.GetAllStars(); // all user's achieved stars (key = year, value = stars)
```

<br>

## Submit answer

```csharp
string status = client.SubmitAnswer(int part, object answer); // submits answer to initialized year and day, returns "True" or "False" or "on cooldown: {cooldown}"
```

<br><br>

*credits to:*
> [Max](https://github.com/Mqxx) - markdown info icons <br>
> [Monday Morning Haskell](https://mmhaskell.com/) - documentation on how to obtaining session cookie <br>
> [Developer.Mozilla](https://developer.mozilla.org) - documentation on how to name Regex groups
