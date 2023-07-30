[![Version](https://img.shields.io/nuget/v/AoCAPI)](https://www.nuget.org/packages/AoCAPI)
[![Downloads](https://img.shields.io/nuget/dt/AoCAPI)](https://www.nuget.org/packages/AoCAPI)

A simple [NuGet](https://nuget.org) package to handle personal [Advent Of Code](https://adventofcode.com) data

## Documentation
- [Adding to .NET project](#adding-to-net-project)
- [Variable Initialization](#variable-initialization)
- [Features](#features)
    - [Get Input Files](#get-input-files)
    - [Get Achieved Stars](#get-achieved-stars)

<br><br>

# Adding to .NET project
Add dependencies to project
```bash
dotnet add package AoCAPI
```

Use in project
```csharp
using AoC.API;
```

<br>

# Variable Initialization
```csharp
var client = new APIHandler("session cookie");
```
> [How to obtain session cookie](https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication)

<br>

# Features

## Get input files

### GetInputText
```csharp
string GetInputText(int year, int day)
string GetInputText(string input, Regex pattern)
```
> <picture>
>   <source media="(prefers-color-scheme: dark)" srcset="https://github.com/Mqxx/GitHub-Markdown/blob/main/blockquotes/badge/dark-theme/info.svg">
>   <img alt="Info" src="https://github.com/Mqxx/GitHub-Markdown/blob/main/blockquotes/badge/dark-theme/Info">
> </picture><br>
> The Regex overload needs to have the group containing the year named "year" and the group containing the day named "day".

> [How to name Regex groups](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions/Named_capturing_group)

#### Usage
```csharp
string input = client.GetInputText(2015, 1); // input file (raw text) from year 2015, day 1
string input = client.GetInputText("2015 day01", new Regex(@"(?<year>\d{4}) day(?<day>\d{2})")); // input file (raw text) from year 2015, day 1
```

### GetInputLines
```csharp
string[] GetInputLines(int year, int day)
string[] GetInputText(string input, Regex pattern)
```
> <picture>
>   <source media="(prefers-color-scheme: dark)" srcset="https://github.com/Mqxx/GitHub-Markdown/blob/main/blockquotes/badge/dark-theme/info.svg">
>   <img alt="Info" src="https://github.com/Mqxx/GitHub-Markdown/blob/main/blockquotes/badge/dark-theme/Info">
> </picture><br>
> The Regex overload needs to have the group containing the year named "year" and the group containing the day named "day".

> [How to name Regex groups](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions/Named_capturing_group)

#### Usage
```csharp
string[] input = client.GetInputLines(2015, 1); // input file (raw lines) from year 2015, day 1
string[] input = client.GetInputLines("2015 day01", new Regex(@"(?<year>\d{4}) day(?<day>\d{2})")); // input file (raw lines) from year 2015, day 1
```

<br>

## Get achieved stars

### Method parameters and return value
```csharp
Dictionary<int, int> GetStars()
```

### Usage
```csharp
Dictionary<int, int> stars = client.GetStars(); // user's achieved stars by year
```

<br><br>

>  credits to: <br>
> [Max](https://github.com/Mqxx) - markdown info icons <br>
> [Monday Morning Haskell](https://mmhaskell.com/) - documentation on how to obtaining session cookie <br>
> [Developer.Mozilla](https://developer.mozilla.org) - documentation on how to name Regex groups