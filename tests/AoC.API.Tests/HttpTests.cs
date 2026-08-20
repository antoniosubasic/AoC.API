using System.Net;
using System.Net.Sockets;
using System.Text;
using AoC.API.Http;

namespace AoC.API.Tests;

/// <summary>
/// The one place a socket is opened. Nothing here leaves the machine: the
/// requests go to a loopback listener the test stands up itself, and no test in
/// this suite contacts adventofcode.com.
/// </summary>
public class HttpTests
{
    [Fact]
    public async Task EveryRequestCarriesTheIdentificationAndTheSessionCookie()
    {
        var (url, sent) = ServeOnce();
        using var transport = Transport(new ClientOptions("53616c7465645f5f", "github.com/me/repo by me@example.com"));

        var response = await transport.ExecuteAsync(TransportRequest.Get(url));

        var request = await sent;
        Assert.Contains("Cookie: session=53616c7465645f5f", request, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("User-Agent: github.com/me/repo by me@example.com", request, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("hello", response.Body);
    }

    [Fact]
    public async Task APostedAnswerIsFormEncoded()
    {
        var (url, sent) = ServeOnce();
        using var transport = Transport(new ClientOptions("cookie", "test-agent"));

        await transport.ExecuteAsync(TransportRequest.PostForm(
            url,
            [new KeyValuePair<string, string>("level", "1"), new KeyValuePair<string, string>("answer", "1 + 1")]));

        var request = await sent;
        Assert.StartsWith("POST /2024/day/1/input", request, StringComparison.Ordinal);
        Assert.Contains("Content-Type: application/x-www-form-urlencoded", request, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("level=1&answer=1+%2B+1", request, StringComparison.Ordinal);
    }

    [Fact]
    public void AnIdentificationThatCannotBeSentIsRejectedBeforeAnyRequest()
    {
        var smuggled = new ClientOptions("cookie", "tool\r\nX-Evil: 1");

        var rejected = Assert.Throws<TransportException>(() => new HttpClientTransport(smuggled));

        Assert.Contains("identification", rejected.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACookieThatCannotBeSentIsRejectedBeforeAnyRequest()
    {
        Assert.Throws<TransportException>(() => new HttpClientTransport(new ClientOptions("bad\ncookie", "test-agent")));
        Assert.Throws<TransportException>(() => new HttpClientTransport(new ClientOptions(string.Empty, "test-agent")));
    }

    [Fact]
    public void ACookieHandedOverWholeIsSentWithItsNameExactlyOnce()
    {
        Assert.Equal("session=53616c7465645f5f", CookieSentFor("53616c7465645f5f"));
        Assert.Equal("session=53616c7465645f5f", CookieSentFor("session=53616c7465645f5f"));

        // Only the leading name is dropped; the value itself is left alone.
        Assert.Equal("session=session=53616c7465645f5f", CookieSentFor("session=session=53616c7465645f5f"));
    }

    [Fact]
    public void ConfiguringAClientTwiceLeavesOneOfEachHeader()
    {
        using var client = new HttpClient();
        var options = new ClientOptions("53616c7465645f5f", "test-agent");

        HttpClientTransport.Configure(client, options);
        HttpClientTransport.Configure(client, options);

        Assert.Single(client.DefaultRequestHeaders.GetValues("Cookie"));
        Assert.Single(client.DefaultRequestHeaders.GetValues("User-Agent"));
    }

    [Fact]
    public void TheCookieNeverAppearsInTheOptionsItWasHandedTo()
    {
        var options = new ClientOptions("53616c7465645f5f", "test-agent");

        Assert.DoesNotContain("53616c7465645f5f", options.ToString(), StringComparison.Ordinal);
        Assert.Contains("<redacted>", options.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ATimeoutThatIsNotATimeoutIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ClientOptions("cookie", "test-agent") { Timeout = TimeSpan.Zero });
        Assert.Equal(TimeSpan.FromSeconds(30), new ClientOptions("cookie", "test-agent").Timeout);
    }

    [Fact]
    public async Task ARequestToNowhereFailsWithTheUrlItWasGiven()
    {
        using var transport = Transport(new ClientOptions("cookie", "test-agent"));

        var failed = await Assert.ThrowsAsync<TransportException>(
            () => transport.ExecuteAsync(TransportRequest.Get("http://127.0.0.1:1/2024/day/1/input")));

        Assert.Equal("http://127.0.0.1:1/2024/day/1/input", failed.Url);
        Assert.NotNull(failed.InnerException);
    }

    [Fact]
    public void AResponseKnowsWhetherItSucceeded()
    {
        Assert.True(TransportResponse.Ok("body").IsSuccess);
        Assert.False(new TransportResponse(400, "body").IsSuccess);
        Assert.False(new TransportResponse(404, "body").IsSuccess);
    }

    private static string CookieSentFor(string cookie)
    {
        using var client = new HttpClient();
        HttpClientTransport.Configure(client, new ClientOptions(cookie, "test-agent"));

        return Assert.Single(client.DefaultRequestHeaders.GetValues("Cookie"));
    }

    /// <summary>
    /// A transport whose client never looks for a proxy, so an ambient
    /// <c>HTTP_PROXY</c> cannot redirect the loopback request. The headers are
    /// built exactly as in production.
    /// </summary>
    private static HttpClientTransport Transport(ClientOptions options) =>
        new(new HttpClient(new SocketsHttpHandler { UseProxy = false }), options);

    /// <summary>
    /// Serves one canned reply on the loopback interface and hands back the
    /// bytes the client sent.
    /// </summary>
    private static (string Url, Task<string> Sent) ServeOnce()
    {
        // Nothing in this suite may wait on the outside world forever: if the
        // client never connects, the listener gives up rather than hanging the
        // run.
        var giveUp = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var url = $"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/2024/day/1/input";

        var sent = Task.Run(async () =>
        {
            try
            {
                using var connection = await listener.AcceptTcpClientAsync(giveUp.Token);
                await using var stream = connection.GetStream();

                var request = await ReadRequestAsync(stream, giveUp.Token);
                await stream.WriteAsync(
                    Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 5\r\nConnection: close\r\n\r\nhello"),
                    giveUp.Token);

                return request;
            }
            finally
            {
                listener.Stop();
                giveUp.Dispose();
            }
        });

        return (url, sent);
    }

    /// <summary>Reads a whole request, head and body, however it is split up on the way.</summary>
    private static async Task<string> ReadRequestAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var request = new StringBuilder();
        var buffer = new byte[8192];

        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read == 0) { break; }

            request.Append(Encoding.UTF8.GetString(buffer, 0, read));
            var text = request.ToString();

            var head = text.IndexOf("\r\n\r\n", StringComparison.Ordinal);
            if (head < 0) { continue; }

            var length = ContentLength(text[..head]);
            if (text.Length - (head + 4) >= length) { return text; }
        }

        return request.ToString();
    }

    private static int ContentLength(string head)
    {
        const string header = "content-length:";

        foreach (var line in head.Split("\r\n"))
        {
            if (line.StartsWith(header, StringComparison.OrdinalIgnoreCase)
                && int.TryParse(line[header.Length..].Trim(), out var length))
            {
                return length;
            }
        }

        return 0;
    }
}
