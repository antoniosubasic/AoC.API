namespace AoC.API;

/// <summary>The puzzle has not unlocked yet.</summary>
public sealed class PuzzleLockedException : AdventOfCodeException
{
    /// <summary>Initializes a new instance of the <see cref="PuzzleLockedException"/> class.</summary>
    public PuzzleLockedException() : base("the puzzle has not unlocked yet") { }

    /// <summary>Initializes a new instance of the <see cref="PuzzleLockedException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    public PuzzleLockedException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="PuzzleLockedException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">What went wrong underneath.</param>
    public PuzzleLockedException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>Initializes a new instance of the <see cref="PuzzleLockedException"/> class.</summary>
    /// <param name="puzzle">The puzzle that is still locked.</param>
    public PuzzleLockedException(Puzzle puzzle) : base($"{puzzle} has not unlocked yet") => Puzzle = puzzle;

    /// <summary>The puzzle that is still locked, when it is known.</summary>
    public Puzzle? Puzzle { get; }
}
