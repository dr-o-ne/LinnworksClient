using System.Net.Http.Json;
using LinnworksClient.Infrastructure.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LinnworksClient.Infrastructure.Default;

internal sealed class LinnworksAuthorizationClient : ILinnworksAuthorizationClient
{
    private const string CacheKey = "Token";
    private readonly TimeSpan _cacheInterval = TimeSpan.FromMinutes(20);

    private readonly IOptions<LinnworksOptions> _options;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    
    public LinnworksAuthorizationClient(
        IOptions<LinnworksOptions> options,
        HttpClient httpClient,
        IMemoryCache memoryCache
    ) {
        _httpClient = httpClient;
        _httpClient.BaseAddress = options.Value.Url;
        _options = options;
        _memoryCache = memoryCache;
    }

    public async Task<LinnworksSession?> Authorize(CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(CacheKey, out LinnworksSession? cachedSession))
            return cachedSession;

        var nvc = new List<KeyValuePair<string, string>>
        {
            new ("ApplicationId", _options.Value.ApplicationId),
            new ("ApplicationSecret", _options.Value.ApplicationSecret),
            new ("Token", _options.Value.Token)
        };

        using var response = await _httpClient.PostAsync("api/Auth/AuthorizeByApplication", new FormUrlEncodedContent(nvc), cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new LinnworksAuthorizationException();

        var session = await response.Content.ReadFromJsonAsync<LinnworksSession>(cancellationToken: cancellationToken);
        _memoryCache.Set(CacheKey, session, _cacheInterval);

        return session;
    }
}