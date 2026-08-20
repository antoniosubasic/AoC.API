namespace AoC.API;

/// <summary>An answer was submitted too recently for this one to be judged.</summary>
/// <remarks>
/// Nothing was judged: the answer still has to be submitted again once the
/// wait is over. An answer that was judged and rejected is a
/// <see cref="Verdict.Incorrect"/>, not this.
/// </remarks>
public sealed class CooldownException : AdventOfCodeException
{
    /// <summary>Initializes a new instance of the <see cref="CooldownException"/> class.</summary>
    public CooldownException() : base("an answer was submitted too recently") { }

    /// <summary>Initializes a new instance of the <see cref="CooldownException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    public CooldownException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="CooldownException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">What went wrong underneath.</param>
    public CooldownException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>Initializes a new instance of the <see cref="CooldownException"/> class.</summary>
    /// <param name="wait">How much of the wait the site says is left.</param>
    public CooldownException(TimeSpan wait)
        : base($"an answer was submitted too recently; {Parsing.Waits.Describe(wait)} left to wait") => Wait = wait;

    /// <summary>
    /// How much of the wait the site says is left. <see cref="TimeSpan.Zero"/>
    /// if the site did not say.
    /// </summary>
    public TimeSpan Wait { get; }
}
