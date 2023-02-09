using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using LinnworksClient.Infrastructure.Default;
using Xunit;

namespace LinnworksClient.Tests.Infrastructure;

public sealed class LinnworksRateLimiterTests
{
    [Fact]
    public async Task Wait_Test()
    {
        const string resourceName = "ReSoUrCeNaMe";
        const int limit = 10;
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        var settings = ImmutableDictionary<string, int>.Empty
            .Add(resourceName, limit);

        var sut = new LinnworksRateLimiter(settings);

        var i = 0;
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            for (i = 0; i < 1000; i++)
            {
                await sut.Wait("resourceName", cts.Token);
            }
        });

        Assert.Equal(limit, i);
    }

}