[![Version](https://img.shields.io/nuget/v/AoCAPI)](https://www.nuget.org/packages/AoCAPI)

A simple [NuGet](https://nuget.org) package to handle personal [Advent Of Code](https://adventofcode.com) data

## Documentation
- [Variable Initialization](#variable-initialization)
- [Features](#features)

<br><br>

# Variable Initialization
```csharp
var client = new APIHandler("session cookie");
```
> [How to obtain session cookie](https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication)

<br>

# Features


## Fetch input files

### Method parameters and return value
```csharp
string GetInput(int year, int day)
```

### Usage
```csharp
string input = client.GetInput(2015, 1); // input file from year 2015, day 1
```

<br>

## Fetch achieved stars

### Method parameters and return value
```csharp
Dictionary<int, int> GetStars()
```

### Usage
```csharp
Dictionary<int, int> stars = client.GetStars(); // user's achieved stars by year
```
