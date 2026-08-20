using AoC.API.Http;

namespace AoC.API.Tests;

public class FakeTransportTests
{
    [Fact]
    public async Task QueuedRepliesComeBackInOrder()
    {
        var transport = new FakeTransport();
        transport.PushBody("first").PushBody("second");

        var first = await transport.ExecuteAsync(TransportRequest.Get("https://example.test/1"));
        var second = await transport.ExecuteAsync(TransportRequest.Get("https://example.test/2"));

        Assert.Equal("first", first.Body);
        Assert.Equal("second", second.Body);
        Assert.Equal(["https://example.test/1", "https://example.test/2"], transport.RequestedUrls);
    }

    [Fact]
    public async Task ARequestWithNothingQueuedFailsLoudly()
    {
        var transport = new FakeTransport();

        var failed = await Assert.ThrowsAsync<TransportException>(
            () => transport.ExecuteAsync(TransportRequest.Get("https://example.test/")));

        Assert.Contains("https://example.test/", failed.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WhatWentOutIsRecordedFieldsAndAll()
    {
        var transport = FakeTransport.Serving("recorded");
        var form = new[]
        {
            new KeyValuePair<string, string>("level", "1"),
            new KeyValuePair<string, string>("answer", "514579"),
        };

        await transport.ExecuteAsync(TransportRequest.PostForm("https://example.test/answer", form));

        var request = Assert.Single(transport.Requests);
        Assert.Equal(TransportRequest.PostForm("https://example.test/answer", form), request);
        Assert.NotEqual(TransportRequest.PostForm("https://example.test/answer", []), request);
    }

    [Fact]
    public async Task RecordedRequestsAreASnapshotRatherThanALiveView()
    {
        var transport = FakeTransport.Serving("first");
        transport.PushBody("second");

        await transport.ExecuteAsync(TransportRequest.Get("https://example.test/1"));
        var recorded = transport.Requests;
        await transport.ExecuteAsync(TransportRequest.Get("https://example.test/2"));

        Assert.Single(recorded);
        Assert.Equal(2, transport.Requests.Count);
    }
}
