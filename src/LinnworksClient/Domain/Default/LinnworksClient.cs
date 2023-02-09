using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using LinnworksClient.Domain.Dto;
using LinnworksClient.Infrastructure;
using static LinnworksClient.Consts;

namespace LinnworksClient.Domain.Default;

internal sealed class LinnworksClient : ILinnworksClient
{
    private const int DefaultBatchSize = 200;
    private const int DefaultConcurrencyLevel = 10;

    private readonly HttpClient _httpClient;
    private readonly ILinnworksRequestFactory _requestFactory;
    private readonly ILinnworksRateLimiter _rateLimiter;

    public LinnworksClient(
        HttpClient httpClient,
        ILinnworksRequestFactory requestFactory,
        ILinnworksRateLimiterFactory rateLimiterFactory
    ) {
        _httpClient = httpClient;
        _requestFactory = requestFactory;
        _rateLimiter = rateLimiterFactory.Create();
    }

    public async Task<int> GetInventoryItemsCount(CancellationToken cancellationToken)
    {
        var nvc = new KeyValuePair<string, string>[]
        {
            new("includeDeleted", "false"),
            new("includeArchived", "false")
        };

        await _rateLimiter.Wait(LinnworksUrl.GetInventoryItemsCount, cancellationToken);
        using var request = await _requestFactory.Create(LinnworksUrl.GetInventoryItemsCount, nvc, cancellationToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

        return result;
    }

    public async Task<IReadOnlyCollection<OpenOrderDto>> GetOpenOrders(CancellationToken cancellationToken)
    {
        using var firstRequest = await _requestFactory.Create(LinnworksUrl.GetOpenOrdersUrl, new List<KeyValuePair<string, string>>
        {
            new("entriesPerPage", DefaultBatchSize.ToString()),
            new("pageNumber", "1"),
        }, cancellationToken);

        await _rateLimiter.Wait(LinnworksUrl.GetOpenOrdersUrl, cancellationToken);
        using var firstResponse = await _httpClient.SendAsync(firstRequest, cancellationToken);
        firstResponse.EnsureSuccessStatusCode();

        var firstResponseContent = await firstResponse.Content.ReadFromJsonAsync<GenericPagedResultDto<OpenOrderDto>>(cancellationToken: cancellationToken);

        var result = new ConcurrentQueue<OpenOrderDto>();
        firstResponseContent?.Data.ForEach(x => result.Enqueue(x));
        var pagesCount = firstResponseContent?.TotalPages ?? 0;

        if (pagesCount > 1)
        {
            var requests = new List<HttpRequestMessage>(pagesCount - 1);
            for (var i = 2; i <= pagesCount; i++)
            {
                requests.Add(await _requestFactory.Create(LinnworksUrl.GetOpenOrdersUrl, new List<KeyValuePair<string, string>>
                {
                    new("entriesPerPage", DefaultBatchSize.ToString()),
                    new("pageNumber", i.ToString()),
                }, cancellationToken));
            }

            var semaphore = new SemaphoreSlim(DefaultConcurrencyLevel);
            var tasks = requests.Select(async request =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    await _rateLimiter.Wait(LinnworksUrl.GetOpenOrdersUrl, cancellationToken);
                    using var response = await _httpClient.SendAsync(request, cancellationToken);

                    var pageData = await response.Content.ReadFromJsonAsync<GenericPagedResultDto<OpenOrderDto>>(cancellationToken: cancellationToken);
                    pageData?.Data.ForEach(x => result.Enqueue(x));
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        return result;
    }

    public async Task<IEnumerable<InventoryItemDto>> GetStockItemsFull(CancellationToken cancellationToken)
    {
        var itemsCount = await GetInventoryItemsCount(cancellationToken);
        if(itemsCount == 0)
            return Array.Empty<InventoryItemDto>();

        var requestsCount = (itemsCount + DefaultBatchSize - 1) / DefaultBatchSize;

        var requests = new List<HttpRequestMessage>(requestsCount);
        for (var i = 0; i < requestsCount; i++)
        {
            var nvc = new List<KeyValuePair<string, string>>
            {
                new("loadCompositeParents", "false"),
                new("loadVariationParents", "false"),
                new("entriesPerPage", DefaultBatchSize.ToString()),
                new("pageNumber", (i + 1).ToString()),
                new("dataRequirements", "[0,6]"),
                new("searchTypes", "[0]")
            };

            await _rateLimiter.Wait(LinnworksUrl.GetStockItemsFull, cancellationToken);
            var request = await _requestFactory.Create(LinnworksUrl.GetStockItemsFull, nvc, cancellationToken);
            
            requests.Add(request);
        }

        var result = new ConcurrentQueue<InventoryItemDto>();

        var semaphore = new SemaphoreSlim(DefaultConcurrencyLevel);
        var tasks = requests.Select(async request =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);

                var items = await response.Content.ReadFromJsonAsync<InventoryItemDto[]>(cancellationToken: cancellationToken);
                if (items != null)
                    foreach (var item in items)
                        result.Enqueue(item);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);

        return result;
    }

    public async Task SetStockLevel(IEnumerable<SetStockLevelDto> dtos, CancellationToken cancellationToken)
    {
        foreach (var chunk in dtos.Chunk(DefaultBatchSize))
        {
            var nvc = new KeyValuePair<string, string>[]
            {
                new ("stockLevels", JsonSerializer.Serialize(chunk))
            };

            await _rateLimiter.Wait(LinnworksUrl.SetStockLevel, cancellationToken);
            using var request = await _requestFactory.Create(LinnworksUrl.SetStockLevel, nvc, cancellationToken);
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();
           
            Console.WriteLine(response.StatusCode);
        }
    }

    public async Task UpdateInventoryItemPrices(IEnumerable<StockItemPriceDto> dtos, CancellationToken cancellationToken)
    {
        foreach (var chunk in dtos.Chunk(DefaultBatchSize))
        {
            var nvc = new KeyValuePair<string, string>[]
            {
                new ("inventoryItemPrices", JsonSerializer.Serialize(chunk))
            };

            await _rateLimiter.Wait(LinnworksUrl.UpdateInventoryItemPrices, cancellationToken);
            using var request = await _requestFactory.Create(LinnworksUrl.UpdateInventoryItemPrices, nvc, cancellationToken);
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();
        }
    }

    public async Task SetOrderShippingInfo(Guid orderId, SetOrderShippingInfoDto info, CancellationToken cancellationToken)
    {
        var nvc = new KeyValuePair<string, string>[]
        {
            new("orderId", orderId.ToString()),
            new("info", JsonSerializer.Serialize(info))
        };

        using var request = await _requestFactory.Create(LinnworksUrl.SetOrderShippingInfo, nvc, cancellationToken);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}