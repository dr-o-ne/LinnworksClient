namespace LinnworksClient.Infrastructure;

internal interface ILinnworksRateLimiter
{
    Task Wait(string url, CancellationToken cancellationToken);
}
