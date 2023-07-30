A simple [NuGet](https://nuget.org) package to handle personal [Advent Of Code](https://adventofcode.com) data

## Documentation
- [Adding to .NET project](#adding-to-net-project)
- [Variable Initialization](#variable-initialization)
- [Features](#features)
    - [Get Input Files](#get-input-files)
    - [Get Achieved Stars](#get-achieved-stars)


# Adding to .NET project
Add dependencies to project
```bash
dotnet add package AoCAPI
```

Use in project
```csharp
using AoC.API;
```


# Variable Initialization
```csharp
var client = new APIHandler("session cookie");
```
> [How to obtain session cookie](https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication)


# Features

## Get input files

### GetInputText
```csharp
string GetInputText(int year, int day)
string GetInputText(string input, Regex pattern)
```
> **INFO**: The Regex overload needs to have the group containing the year named "year" and the group containing the day named "day".

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
> **INFO**: The Regex overload needs to have the group containing the year named "year" and the group containing the day named "day".

> [How to name Regex groups](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Regular_expressions/Named_capturing_group)

#### Usage
```csharp
string[] input = client.GetInputLines(2015, 1); // input file (raw lines) from year 2015, day 1
string[] input = client.GetInputLines("2015 day01", new Regex(@"(?<year>\d{4}) day(?<day>\d{2})")); // input file (raw lines) from year 2015, day 1
```


## Get achieved stars

### Method parameters and return value
```csharp
Dictionary<int, int> GetStars()
```

### Usage
```csharp
Dictionary<int, int> stars = client.GetStars(); // user's achieved stars by year
```


>  credits to:
> [Max](https://github.com/Mqxx) - markdown info icons
> [Monday Morning Haskell](https://mmhaskell.com/) - documentation on how to obtaining session cookie
> [Developer.Mozilla](https://developer.mozilla.org) - documentation on how to name Regex groups