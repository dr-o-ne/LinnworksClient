using System.Collections.Immutable;
using static LinnworksClient.Consts;

namespace LinnworksClient.Infrastructure.Default;

internal sealed class LinnworksRateLimiterFactory : ILinnworksRateLimiterFactory
{
    public ILinnworksRateLimiter Create()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, int>(StringComparer.OrdinalIgnoreCase);

        builder.Add(LinnworksUrl.GetInventoryItemsCount, 150);
        builder.Add(LinnworksUrl.SetStockLevel, 250);
        builder.Add(LinnworksUrl.UpdateInventoryItemPrices, 250);
        builder.Add(LinnworksUrl.GetStockItemsFull, 150);
        builder.Add(LinnworksUrl.GetOpenOrdersUrl, 250);
        builder.Add(LinnworksUrl.SetOrderShippingInfo, 250);

        return new LinnworksRateLimiter(builder.ToImmutable());
    }
}
