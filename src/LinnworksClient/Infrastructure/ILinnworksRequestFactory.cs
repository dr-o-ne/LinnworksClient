namespace LinnworksClient.Infrastructure;

internal interface ILinnworksRequestFactory
{
    Task<HttpRequestMessage> Create
    (
        string url,
        IEnumerable<KeyValuePair<string, string>> nvc,
        CancellationToken cancellationToken
    );
}
