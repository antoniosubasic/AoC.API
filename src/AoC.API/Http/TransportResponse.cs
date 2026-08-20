namespace AoC.API.Http;

/// <summary>A reply from Advent of Code.</summary>
/// <param name="StatusCode">The HTTP status code.</param>
/// <param name="Body">The body, decoded as text.</param>
public sealed record TransportResponse(int StatusCode, string Body)
{
    /// <summary>Whether the status is in the <c>2xx</c> range.</summary>
    public bool IsSuccess => StatusCode is >= 200 and < 300;

    /// <summary>A <c>200 OK</c> reply carrying <paramref name="body"/>.</summary>
    /// <param name="body">The body.</param>
    /// <returns>The reply.</returns>
    public static TransportResponse Ok(string body) => new(200, body);
}
