namespace LinnworksClient.Infrastructure;

internal interface ILinnworksRateLimiterFactory
{
    ILinnworksRateLimiter Create();
}
