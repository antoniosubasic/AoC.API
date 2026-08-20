namespace AoC.API.Http;

/// <summary>
/// A <see cref="ITransport"/> that answers from a queue instead of from the
/// network.
/// </summary>
/// <remarks>
/// Every endpoint and every parser in this library is exercised through this
/// type, so its test suite needs neither a network nor a session cookie. It
/// ships in the package because a tool built on this library needs the same
/// seam to test itself.
/// </remarks>
/// <example>
/// <code>
/// var transport = FakeTransport.Serving("1721\n979\n366\n");
/// using var session = new Session(transport);
///
/// var input = await session.GetInputTextAsync(Puzzle.At(2020, 1));
///
/// Assert.Equal("1721\n979\n366", input);
/// Assert.Equal(["https://adventofcode.com/2020/day/1/input"], transport.RequestedUrls);
/// </code>
/// </example>
public sealed class FakeTransport : ITransport
{
    private readonly Queue<TransportResponse> _replies = new();
    private readonly List<TransportRequest> _requests = [];
    private readonly object _gate = new();

    /// <summary>A transport whose first reply is <c>200 OK</c> with <paramref name="body"/>.</summary>
    /// <param name="body">The body to reply with.</param>
    /// <returns>The transport.</returns>
    public static FakeTransport Serving(string body)
    {
        var transport = new FakeTransport();
        transport.PushBody(body);

        return transport;
    }

    /// <summary>Every request made so far, in order.</summary>
    public IReadOnlyList<TransportRequest> Requests
    {
        get { lock (_gate) { return [.. _requests]; } }
    }

    /// <summary>The URL of every request made so far, in order.</summary>
    public IReadOnlyList<string> RequestedUrls
    {
        get { lock (_gate) { return [.. _requests.Select(request => request.Url)]; } }
    }

    /// <summary>Queues a reply behind whatever is already queued.</summary>
    /// <param name="response">The reply to queue.</param>
    /// <returns>This transport, so replies can be queued one after another.</returns>
    public FakeTransport Push(TransportResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        lock (_gate) { _replies.Enqueue(response); }

        return this;
    }

    /// <summary>Queues a <c>200 OK</c> reply carrying <paramref name="body"/>.</summary>
    /// <param name="body">The body to reply with.</param>
    /// <returns>This transport, so replies can be queued one after another.</returns>
    public FakeTransport PushBody(string body) => Push(TransportResponse.Ok(body));

    /// <inheritdoc />
    public Task<TransportResponse> ExecuteAsync(
        TransportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        TransportResponse? reply;
        lock (_gate)
        {
            _requests.Add(request);
            reply = _replies.Count > 0 ? _replies.Dequeue() : null;
        }

        return reply is null
            ? throw new TransportException(
                $"no reply was queued for the request to {request.Url}",
                request.Url,
                innerException: null)
            : Task.FromResult(reply);
    }
}
