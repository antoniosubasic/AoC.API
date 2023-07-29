A simple [NuGet](https://nuget.org) package to handle personal [Advent Of Code](https://adventofcode.com) data

## Documentation
- [Variable Initialization](#variable-initialization)
- [Features](#features)


# Variable Initialization
```csharp
var client = new APIHandler("session cookie");
```
> [How to obtain session cookie](https://mmhaskell.com/blog/2023/1/30/advent-of-code-fetching-puzzle-input-using-the-api#authentication)


# Features


## Fetch input files

### Method parameters and return value
```csharp
string GetInputText(int year, int day)
string[] GetInputLines(int year, int day)
```

### Usage
```csharp
string input = client.GetInputText(2015, 1); // input file (raw text) from year 2015, day 1
string[] input = client.GetInputLines(2015, 1); // input file (raw lines) from year 2015, day 1
```


## Fetch achieved stars

### Method parameters and return value
```csharp
Dictionary<int, int> GetStars()
```

### Usage
```csharp
Dictionary<int, int> stars = client.GetStars(); // user's achieved stars by year
```