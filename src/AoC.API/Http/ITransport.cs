namespace AoC.API.Http;

/// <summary>
/// Sends requests to Advent of Code.
/// </summary>
/// <remarks>
/// This is the seam the rest of the library is built on: no endpoint and no
/// parser knows how a reply was obtained, so every one of them can be driven
/// from <see cref="FakeTransport"/> in a test - or from a transport of your own
/// that caches, throttles or records.
/// </remarks>
public interface ITransport
{
    /// <summary>Sends <paramref name="request"/> and returns the reply.</summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">Abandons the request when signalled.</param>
    /// <returns>The reply, whatever its status.</returns>
    /// <exception cref="TransportException">
    /// The request could not be completed. A reply with an unsuccessful status
    /// is a <see cref="TransportResponse"/>, not an exception; interpreting the
    /// status is the caller's job.
    /// </exception>
    Task<TransportResponse> ExecuteAsync(TransportRequest request, CancellationToken cancellationToken = default);
}
