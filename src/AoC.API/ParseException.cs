namespace AoC.API;

/// <summary>The reply could not be interpreted.</summary>
/// <remarks>
/// Advent of Code has no API, so every reply is a page meant for a browser.
/// This is what a reply this library does not recognise looks like, which
/// usually means the site changed its wording.
/// </remarks>
public sealed class ParseException : AdventOfCodeException
{
    /// <summary>Initializes a new instance of the <see cref="ParseException"/> class.</summary>
    public ParseException() : base("the reply could not be interpreted") { }

    /// <summary>Initializes a new instance of the <see cref="ParseException"/> class.</summary>
    /// <param name="message">What could not be read.</param>
    public ParseException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="ParseException"/> class.</summary>
    /// <param name="message">What could not be read.</param>
    /// <param name="innerException">What went wrong underneath.</param>
    public ParseException(string message, Exception innerException) : base(message, innerException) { }
}
