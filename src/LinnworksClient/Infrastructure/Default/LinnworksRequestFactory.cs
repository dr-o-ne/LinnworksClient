namespace LinnworksClient.Infrastructure.Default;

internal sealed class LinnworksRequestFactory : ILinnworksRequestFactory
{
    private readonly ILinnworksAuthorizationClient _authorizationClient;

    public LinnworksRequestFactory(ILinnworksAuthorizationClient authorizationClient)
    {
        _authorizationClient = authorizationClient;
    }

    public async Task<HttpRequestMessage> Create
    (
        string url,
        IEnumerable<KeyValuePair<string, string>> nvc,
        CancellationToken cancellationToken
    ) {
        var session = await _authorizationClient.Authorize(cancellationToken);

        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(session!.Server + url));
        request.Headers.Add("Authorization", session.Token);
        request.Content = new FormUrlEncodedContent(nvc);

        return request;
    }
}