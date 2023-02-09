using System;
using System.Threading;
using System.Threading.Tasks;
using LinnworksClient.Infrastructure;
using LinnworksClient.Infrastructure.Dto;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace LinnworksClient.Tests.Infrastructure;

public sealed class LinnworksAuthorizationClientTests : IDisposable
{
    private const string ResponseBody200 = @"{""Token"":""0cc5c408-c907-45ad-8e80-21dfedce9bbd"",""Server"":""https://eu-ext.linnworks.net""}";
    private const string ResponseBody401 = @"{""Code"":null,""Message"":""This application has been deleted and can no longer be authorized""}";

    private readonly WireMockServer _server;

    private readonly ILinnworksAuthorizationClient _sut;

    private readonly IRequestBuilder _request = Request.Create()
        .UsingPost()
        .WithUrl("*/api/Auth/AuthorizeByApplication");

    private readonly IResponseBuilder _response200 = Response.Create()
        .WithStatusCode(200)
        .WithHeader("Content-Type", "application/json")
        .WithBody(ResponseBody200);

    private readonly IResponseBuilder _response401 = Response.Create()
        .WithStatusCode(401)
        .WithHeader("Content-Type", "application/json")
        .WithBody(ResponseBody401);

    private readonly IResponseBuilder _response404 = Response.Create()
        .WithStatusCode(404)
        .WithHeader("Content-Type", "application/json");

    public LinnworksAuthorizationClientTests()
    {
        _server = WireMockServer.Start();

        var services = new ServiceCollection();
        services.RegisterTestLinnworksClient(_server.Url!);
        var sp = services.BuildServiceProvider();

        _sut = sp.GetRequiredService<ILinnworksAuthorizationClient>();
    }

    [Fact]
    public async Task Authorize_Success_Test()
    {
        _server
            .Given(_request)
            .RespondWith(_response200);

        var session = await _sut.Authorize(default);

        Assert.Equal("0cc5c408-c907-45ad-8e80-21dfedce9bbd", session!.Token);
        Assert.Equal("https://eu-ext.linnworks.net", session.Server);
    }

    [Fact]
    public async Task Authorize_Cache_Test()
    {
        _server
            .Given(_request)
            .RespondWith(_response200);

        var session1 = await _sut.Authorize(default);
        var session2 = await _sut.Authorize(default);
        var session3 = await _sut.Authorize(default);

        Assert.NotNull(session1);
        Assert.Equal(session1, session2);
        Assert.Equal(session2, session3);
        Assert.Single(_server.LogEntries);
    }

    [Fact]
    public async Task Authorize_NonAuthorized_Test()
    {
        _server
            .Given(_request)
            .RespondWith(_response401);

        await Assert.ThrowsAsync<LinnworksAuthorizationException>(() => _sut.Authorize(default));
    }

    [Fact]
    public async Task Authorize_Retry_Test()
    {
        _server
            .Given(_request)
            .InScenario("Retry")
            .WillSetStateTo("Try2")
            .RespondWith(_response404);

        _server
            .Given(_request)
            .InScenario("Retry")
            .WhenStateIs("Try2")
            .RespondWith(_response200);

        var session = await _sut.Authorize(default);

        Assert.NotNull(session);
    }

    [Fact]
    public async Task Authorize_Cancellation_Test()
    {
        _server
            .Given(_request)
            .RespondWith(_response200);

        await Assert.ThrowsAsync<TaskCanceledException>(() => _sut.Authorize(new CancellationToken(true)));

        Assert.Empty(_server.LogEntries);
    }

    public void Dispose()
    {
        _server.Dispose();
    }

}
