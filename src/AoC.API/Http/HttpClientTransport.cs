namespace AoC.API.Http;

/// <summary>
/// The real transport, backed by <c>adventofcode.com</c>.
/// </summary>
/// <remarks>
/// This is the only place an <see cref="HttpClient"/> is configured, and
/// <see cref="Configure"/> is the only place the identification the Advent of
/// Code automation guidelines ask for and the session cookie are set. They are
/// default headers on the client rather than headers a call site adds, so no
/// request - present or future, from this library or from anything built on it -
/// can leave without them.
/// </remarks>
public sealed class HttpClientTransport : ITransport, IDisposable
{
    private readonly HttpClient _client;
    private readonly bool _ownsClient;

    /// <summary>Initializes a new instance of the <see cref="HttpClientTransport"/> class.</summary>
    /// <param name="options">The cookie, the identification and the timeout.</param>
    /// <exception cref="TransportException">The cookie or the identification cannot be sent in a header.</exception>
    public HttpClientTransport(ClientOptions options) : this(new HttpClient(), options, ownsClient: true) { }

    /// <summary>Initializes a new instance of the <see cref="HttpClientTransport"/> class.</summary>
    /// <remarks>
    /// The client is configured for this transport and is not disposed with it,
    /// which is what a client from an <c>IHttpClientFactory</c> wants. Give this
    /// transport a client of its own: its default headers carry a credential.
    /// </remarks>
    /// <param name="client">The client to send through.</param>
    /// <param name="options">The cookie, the identification and the timeout.</param>
    /// <exception cref="TransportException">The cookie or the identification cannot be sent in a header.</exception>
    public HttpClientTransport(HttpClient client, ClientOptions options)
        : this(client, options, ownsClient: false) { }

    private HttpClientTransport(HttpClient client, ClientOptions options, bool ownsClient)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            Configure(client, options);
        }
        catch
        {
            if (ownsClient) { client.Dispose(); }
            throw;
        }

        _client = client;
        _ownsClient = ownsClient;
    }

    /// <summary>
    /// Puts the identification and the session cookie on a client, replacing
    /// whatever was there.
    /// </summary>
    /// <param name="client">The client to configure.</param>
    /// <param name="options">The cookie, the identification and the timeout.</param>
    /// <exception cref="TransportException">Either value cannot be sent in a header.</exception>
    public static void Configure(HttpClient client, ClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        if (!CanBeSentInAHeader(options.Identification))
        {
            throw new TransportException(
                $"the identification \"{options.Identification}\" is empty or holds characters an http header cannot carry");
        }

        if (!CanBeSentInAHeader(options.Cookie))
        {
            throw new TransportException(
                "the session cookie is empty or holds characters an http header cannot carry");
        }

        client.Timeout = options.Timeout;
        client.DefaultRequestHeaders.Remove(HeaderNames.UserAgent);
        client.DefaultRequestHeaders.Remove(HeaderNames.Cookie);

        // The guidelines ask for `github.com/my-repo by me@example.com`, which
        // is not a product token and would not survive header validation - so it
        // goes on unvalidated, having been checked above.
        client.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.UserAgent, options.Identification);
        client.DefaultRequestHeaders.TryAddWithoutValidation(
            HeaderNames.Cookie,
            ClientOptions.CookiePrefix + options.Cookie);
    }

    /// <inheritdoc />
    public async Task<TransportResponse> ExecuteAsync(
        TransportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var message = Build(request);

        try
        {
            using var response = await _client.SendAsync(message, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            return new TransportResponse((int)response.StatusCode, body);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception failure) when (failure is HttpRequestException or OperationCanceledException or IOException)
        {
            throw new TransportException($"the request to {request.Url} failed", request.Url, failure);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsClient) { _client.Dispose(); }
    }

    /// <summary>Turns one of this library's requests into an <see cref="HttpRequestMessage"/>.</summary>
    private static HttpRequestMessage Build(TransportRequest request)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var url))
        {
            throw new TransportException($"{request.Url} is not an absolute url", request.Url, innerException: null);
        }

        var message = new HttpRequestMessage(request.Method == RequestMethod.Post ? HttpMethod.Post : HttpMethod.Get, url);

        if (request.Method == RequestMethod.Post)
        {
            message.Content = new FormUrlEncodedContent(request.Form);
        }

        return message;
    }

    /// <summary>
    /// Whether a value can go into a header as written.
    /// </summary>
    /// <remarks>
    /// Visible ASCII only, which rules out the newline that would otherwise let
    /// an identification smuggle a header of its own in.
    /// </remarks>
    private static bool CanBeSentInAHeader(string value) =>
        value.Length > 0 && value.All(character => character is >= ' ' and <= '~');

    private static class HeaderNames
    {
        public const string Cookie = "Cookie";
        public const string UserAgent = "User-Agent";
    }
}
