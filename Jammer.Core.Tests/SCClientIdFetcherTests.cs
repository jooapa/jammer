using System.Net;

namespace Jammer.Core.Tests;

public sealed class SCClientIdFetcherTests
{
    private const string IdA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456";
    private const string IdB = "1234567890ABCDEFGHIJKLMNOPQRSTUV";

    [Fact]
    public async Task FetchesClientIdFromScript()
    {
        var handler = new StubHandler(request => request.RequestUri!.Host == "soundcloud.com"
            ? Html("<script src=\"https://a-v2.sndcdn.com/assets/app.js\"></script>")
            : JavaScript($"client_id:\"{IdA}\""));

        string result = await new SCClientIdFetcher(new HttpClient(handler)).FetchAsync();

        Assert.Equal(IdA, result);
    }

    [Fact]
    public async Task SearchesScriptsInReverseOrderAndStopsAfterMatch()
    {
        int firstScriptRequests = 0;
        var handler = new StubHandler(request => request.RequestUri!.Host == "soundcloud.com"
            ? Html("<script src='https://a-v2.sndcdn.com/assets/first.js'></script>" +
                   "<script src='https://a-v2.sndcdn.com/assets/last.js'></script>")
            : request.RequestUri.AbsolutePath.EndsWith("last.js")
                ? JavaScript($"client_id='{IdB}'")
                : CountedJavaScript(ref firstScriptRequests, $"client_id='{IdA}'"));

        string result = await new SCClientIdFetcher(new HttpClient(handler)).FetchAsync();

        Assert.Equal(IdB, result);
        Assert.Equal(0, firstScriptRequests);
    }

    [Fact]
    public async Task ReportsNoScripts()
    {
        var fetcher = new SCClientIdFetcher(new HttpClient(new StubHandler(_ => Html("<html></html>"))));
        await Assert.ThrowsAsync<InvalidOperationException>(() => fetcher.FetchAsync());
    }

    [Fact]
    public async Task ReportsNoClientId()
    {
        var handler = new StubHandler(request => request.RequestUri!.Host == "soundcloud.com"
            ? Html("<script src='https://a-v2.sndcdn.com/assets/app.js'></script>")
            : JavaScript("const nothing = true;"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new SCClientIdFetcher(new HttpClient(handler)).FetchAsync());
    }

    [Theory]
    [InlineData("client_id='short'")]
    [InlineData("client_id='ABCDEFGHIJKLMNOPQRSTUVWXYZ12345!'")]
    [InlineData("client_id='ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567'")]
    public void RejectsInvalidClientIds(string script) => Assert.Null(SCClientIdFetcher.ExtractClientId(script));

    [Fact]
    public async Task PropagatesHttpFailure()
    {
        var fetcher = new SCClientIdFetcher(new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.BadGateway))));
        await Assert.ThrowsAsync<HttpRequestException>(() => fetcher.FetchAsync());
    }

    [Fact]
    public async Task HonorsTimeout()
    {
        using var client = new HttpClient(new StubHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return Html("");
        })) { Timeout = TimeSpan.FromMilliseconds(25) };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new SCClientIdFetcher(client).FetchAsync());
    }

    [Fact]
    [Trait("Category", "Live")]
    public async Task LiveSmokeTestWhenExplicitlyEnabled()
    {
        if (Environment.GetEnvironmentVariable("JAMMER_RUN_LIVE_TESTS") != "1") return;
        string clientId = await new SCClientIdFetcher().FetchAsync();
        Assert.Matches("^[A-Za-z0-9]{32}$", clientId);
    }

    private static HttpResponseMessage Html(string value) => new(HttpStatusCode.OK) { Content = new StringContent(value) };
    private static HttpResponseMessage JavaScript(string value) => new(HttpStatusCode.OK) { Content = new StringContent(value) };
    private static HttpResponseMessage CountedJavaScript(ref int count, string value) { count++; return JavaScript(value); }
}

internal sealed class StubHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

    public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : this((request, _) => Task.FromResult(handler(request))) { }
    public StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) => _handler = handler;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        _handler(request, cancellationToken);
}
