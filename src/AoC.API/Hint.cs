namespace AoC.API;

/// <summary>Which way a rejected answer was wrong, when the site says.</summary>
public enum Hint
{
    /// <summary>The answer is larger than the right one.</summary>
    TooHigh,

    /// <summary>The answer is smaller than the right one.</summary>
    TooLow,
}
