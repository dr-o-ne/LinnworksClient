using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LinnworksClient.Domain;
using LinnworksClient.Domain.Dto;
using LinnworksClient.Infrastructure;
using LinnworksClient.Infrastructure.Dto;
using LinnworksClient.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace LinnworksClient.Tests.Domain;

public sealed class LinnworksClientTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly ILinnworksClient _sut;

    private readonly IRequestBuilder _getInventoryItemsCountRequest = Request.Create()
        .UsingPost()
        .WithUrl("*/api/Inventory/GetInventoryItemsCount");

    private readonly IRequestBuilder _setStockLevelRequest = Request.Create()
        .UsingPost()
        .WithUrl("*/api/Stock/SetStockLevel");

    private readonly IRequestBuilder _updateInventoryItemPricesRequest = Request.Create()
        .UsingPost()
        .WithUrl("*/api/Inventory/UpdateInventoryItemPrices");

    private readonly IResponseBuilder _response200 = Response.Create()
        .WithStatusCode(200)
        .WithHeader("Content-Type", "application/json");

    public LinnworksClientTests()
    {
        _server = WireMockServer.Start();

        var authorizationClientMock = new Mock<ILinnworksAuthorizationClient>();
        authorizationClientMock
            .Setup(x => x.Authorize(default))
            .ReturnsAsync(new LinnworksSession(_server.Url!, Guid.NewGuid().ToString()));

        var services = new ServiceCollection();
        services.RegisterTestLinnworksClient(_server.Url!);
        services.AddSingleton(authorizationClientMock.Object);
        var sp = services.BuildServiceProvider();

        _sut = sp.GetRequiredService<ILinnworksClient>();
    }

    [Fact]
    public async Task GetOpenOrders_Success_Test()
    {
        const int pageSize = 200;
        const int total = 777;
        const int pages =  total / pageSize + (total % pageSize != 0 ? 1 : 0);

        for (var page = 0; page < pages; page++)
        {
            var request = Request.Create()
                .UsingPost()
                .WithUrl("*/api/Orders/GetOpenOrders")
                .WithBody($"entriesPerPage={pageSize}&pageNumber={page + 1}");

            var response = Response.Create()
                .WithStatusCode(pageSize)
                .WithHeader("Content-Type", "application/json")
                .WithBody(JsonSerializer.Serialize(new GenericPagedResultDto<OpenOrderDto>
                {
                    EntriesPerPage = pageSize,
                    PageNumber = page + 1,
                    TotalEntries = total,
                    TotalPages = pages,
                    Data = OpenOrderGenerator.GenerateOrders(page + 1 == pages ? total - pageSize * page : pageSize, pageSize * page)
                }));

            _server
                .Given(request)
                .RespondWith(response);
        }

        var result = await _sut.GetOpenOrders(default);

        Assert.Equal(total, result.Count);
    }

    [Fact]
    public async Task GetInventoryItemsCount_Success_Test()
    {
        _server
            .Given(_getInventoryItemsCountRequest)
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("230"));

        var result = await _sut.GetInventoryItemsCount(default);

        Assert.Equal(230, result);
    }

    [Fact]
    public async Task SetStockLevel_Batch_Test()
    {
        _server
            .Given(_setStockLevelRequest)
            .RespondWith(_response200);

        var items = Enumerable.Range(0, 500)
            .Select(x => new SetStockLevelDto(x.ToString(), "", x));

        await _sut.SetStockLevel(items, default);

        Assert.Equal(3, _server.LogEntries.Count());
    }

    [Fact]
    public async Task UpdateInventoryItemPrices_Batch_Test()
    {
        _server
            .Given(_updateInventoryItemPricesRequest)
            .RespondWith(_response200);

        var items = Enumerable.Range(0, 500)
            .Select(x => new StockItemPriceDto(Guid.NewGuid(), "", "", Guid.NewGuid(), x));

        await _sut.UpdateInventoryItemPrices(items, default);

        Assert.Equal(3, _server.LogEntries.Count());
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}
