using System.Collections.Immutable;
using System.Globalization;
using System.Threading.RateLimiting;
using LinnworksClient.Infrastructure.Dto;

namespace LinnworksClient.Infrastructure.Default;

internal sealed class LinnworksRateLimiter : ILinnworksRateLimiter
{
    private readonly ImmutableDictionary<string, TokenBucketRateLimiter> _rateLimiters;

    public LinnworksRateLimiter(ImmutableDictionary<string, int> settings)
    {
        var defaultReplenishmentPeriod = TimeSpan.FromMinutes(1);
        var builder = ImmutableDictionary.CreateBuilder<string, TokenBucketRateLimiter>(StringComparer.OrdinalIgnoreCase);

        foreach (var (url, limit) in settings)
        {
            builder.Add(url, new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = limit,
                TokensPerPeriod = limit,
                AutoReplenishment = true,
                ReplenishmentPeriod = defaultReplenishmentPeriod
            }));
        }

        _rateLimiters = builder.ToImmutable();
    }

    public async Task Wait(string url, CancellationToken cancellationToken)
    {
        if(!_rateLimiters.TryGetValue(url, out var rateLimiter))
            throw new ArgumentException(nameof(url));

        RateLimitLease? lease = null;
        try
        {
            lease = rateLimiter.AttemptAcquire();
            while (!lease.IsAcquired)
            {
                var delay = GetRetryAfterDelay(lease);
                await Task.Delay(delay, cancellationToken);

                lease = rateLimiter.AttemptAcquire();
            }
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch(Exception ex)
        {
            throw new LinnworksRateLimitException(string.Empty, ex);
        }
        finally
        {
            lease?.Dispose();
        }
    }

    private static TimeSpan GetRetryAfterDelay(RateLimitLease lease)
    {
        lease.TryGetMetadata("RETRY_AFTER", out var timeSpanValue);
        return TimeSpan.ParseExact(timeSpanValue!.ToString()!, "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
    }
}
