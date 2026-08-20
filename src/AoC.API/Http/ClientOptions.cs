using System.Globalization;

namespace AoC.API.Http;

/// <summary>
/// Everything the one HTTP client needs to know.
/// </summary>
/// <remarks>
/// The cookie is a credential: it is never written to <see cref="ToString"/>,
/// so an options object can be logged without leaking it.
/// </remarks>
public sealed class ClientOptions
{
    /// <summary>What the session credential is sent under.</summary>
    internal const string CookiePrefix = "session=";

    private readonly TimeSpan _timeout = DefaultTimeout;

    /// <summary>Initializes a new instance of the <see cref="ClientOptions"/> class.</summary>
    /// <param name="cookie">
    /// The value of the <c>session</c> cookie on <c>adventofcode.com</c> while
    /// logged in, with or without a leading <c>session=</c>.
    /// </param>
    /// <param name="identification">
    /// How every request identifies you, as the Advent of Code automation
    /// guidelines ask - <c>github.com/my-username/my-repo by me@example.com</c>
    /// or similar.
    /// </param>
    public ClientOptions(string cookie, string identification)
    {
        ArgumentNullException.ThrowIfNull(cookie);
        ArgumentNullException.ThrowIfNull(identification);

        Cookie = cookie.StartsWith(CookiePrefix, StringComparison.Ordinal)
            ? cookie[CookiePrefix.Length..]
            : cookie;
        Identification = identification;
    }

    /// <summary>How long a request may take before it is abandoned.</summary>
    public static TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(30);

    /// <summary>How every request identifies you.</summary>
    public string Identification { get; }

    /// <summary>How long a request may take before it is abandoned.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The timeout is not positive.</exception>
    public TimeSpan Timeout
    {
        get => _timeout;
        init
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, TimeSpan.Zero);
            _timeout = value;
        }
    }

    /// <summary>The session cookie, without its name.</summary>
    internal string Cookie { get; }

    /// <inheritdoc />
    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"ClientOptions {{ Cookie = <redacted>, Identification = {Identification}, Timeout = {Timeout} }}");
}
