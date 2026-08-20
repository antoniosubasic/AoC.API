namespace AoC.API;

/// <summary>Advent of Code replied with a status this library does not expect.</summary>
public sealed class UnexpectedStatusException : AdventOfCodeException
{
    /// <summary>Initializes a new instance of the <see cref="UnexpectedStatusException"/> class.</summary>
    public UnexpectedStatusException() : base("advent of code replied with an unexpected http status") { }

    /// <summary>Initializes a new instance of the <see cref="UnexpectedStatusException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    public UnexpectedStatusException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="UnexpectedStatusException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">What went wrong underneath.</param>
    public UnexpectedStatusException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>Initializes a new instance of the <see cref="UnexpectedStatusException"/> class.</summary>
    /// <param name="statusCode">The status the site replied with.</param>
    public UnexpectedStatusException(int statusCode)
        : base(FormattableString.Invariant($"advent of code replied with http status {statusCode}")) =>
        StatusCode = statusCode;

    /// <summary>The status the site replied with.</summary>
    public int StatusCode { get; }
}
