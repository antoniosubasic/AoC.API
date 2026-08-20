namespace AoC.API;

/// <summary>The session cookie is missing, expired or not accepted.</summary>
public sealed class UnauthorizedException : AdventOfCodeException
{
    /// <summary>Initializes a new instance of the <see cref="UnauthorizedException"/> class.</summary>
    public UnauthorizedException()
        : base("advent of code did not accept the session cookie; it is missing, expired or invalid") { }

    /// <summary>Initializes a new instance of the <see cref="UnauthorizedException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    public UnauthorizedException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="UnauthorizedException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">What went wrong underneath.</param>
    public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
}
