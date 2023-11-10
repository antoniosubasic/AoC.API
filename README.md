[![Version](https://img.shields.io/nuget/v/AoCAPI)](https://www.nuget.org/packages/AoCAPI)
[![Downloads](https://img.shields.io/nuget/dt/AoCAPI)](https://www.nuget.org/packages/AoCAPI)

A simple [NuGet](https://nuget.org) package to handle personal [AoC](https://adventofcode.com) data directly from your .NET project

## Documentation

- [Add to your project](#add-to-your-project)
- [Initialization](#initialization)
- [Features](#features)
    - [Get Input Files](#get-input-file)
    - [Get Achieved Stars](#get-achieved-stars)
    - [Submit Answer](#submit-answer)

<br><br>

# Add to your project

```bash
dotnet add package AoCAPI
```

```csharp
using AoC.API;
```

<br>

# Initialization

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
> The Regex overload needs to have the group containing the year named "year" and the group containing the day named "day".

> [How to obtain session cookie](https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication)

> [How to name Regex groups](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions/Named_capturing_group)

<br>

# Features

## Get input file

```csharp
string inputText = client.GetInputText(); // input file (raw text)
string inputLines = client.GetInputLines(); // input file (lines)
```

<br>

## Get achieved stars

```csharp
Dictionary<int, int> allStars = client.GetAllStars(); // all user's achieved stars (key = year, value = stars)
```

<br>

## Submit answer

```csharp
bool succeeded = client.SubmitAnswer(int level, object answer); // submits answer to initialized year and day, returns true if answer is correct
```

<br><br>

> credits to:<br> > [Max](https://github.com/Mqxx) - markdown info icons<br> > [Monday Morning Haskell](https://mmhaskell.com/) - documentation on how to obtaining session cookie<br> > [Developer.Mozilla](https://developer.mozilla.org) - documentation on how to name Regex groups
