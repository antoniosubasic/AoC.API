namespace AoC.API;

/// <summary>
/// Anything that stops a call to Advent of Code from producing an answer.
/// </summary>
/// <remarks>
/// Every exception this library throws while talking to the site derives from
/// this one, so a caller that does not want to tell them apart can catch this
/// and be sure nothing escapes. Coordinates that do not name a puzzle are the
/// exception: they are a mistake in an argument rather than something the site
/// did, so they are a <see cref="PuzzleException"/>.
/// </remarks>
public abstract class AdventOfCodeException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="AdventOfCodeException"/> class.</summary>
    protected AdventOfCodeException() { }

    /// <summary>Initializes a new instance of the <see cref="AdventOfCodeException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    protected AdventOfCodeException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="AdventOfCodeException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">What went wrong underneath, if anything.</param>
    protected AdventOfCodeException(string message, Exception? innerException) : base(message, innerException) { }
}
