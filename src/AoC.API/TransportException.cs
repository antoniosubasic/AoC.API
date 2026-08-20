namespace AoC.API;

/// <summary>The request never completed.</summary>
public sealed class TransportException : AdventOfCodeException
{
    /// <summary>Initializes a new instance of the <see cref="TransportException"/> class.</summary>
    public TransportException() : base("the request failed") { }

    /// <summary>Initializes a new instance of the <see cref="TransportException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    public TransportException(string message) : base(message) { }

    /// <summary>Initializes a new instance of the <see cref="TransportException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="innerException">What went wrong underneath.</param>
    public TransportException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>Initializes a new instance of the <see cref="TransportException"/> class.</summary>
    /// <param name="message">What went wrong.</param>
    /// <param name="url">The URL that was being requested.</param>
    /// <param name="innerException">What went wrong underneath, if anything.</param>
    public TransportException(string message, string? url, Exception? innerException)
        : base(message, innerException) => Url = url;

    /// <summary>The URL that was being requested, when there was one.</summary>
    public string? Url { get; }
}
