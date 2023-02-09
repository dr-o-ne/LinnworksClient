using System.Text.Json.Serialization;

namespace LinnworksClient.Domain.Dto;

public sealed record InventoryItemDto
(
    [property: JsonPropertyName("StockItemId")]
    Guid StockItemId,

    [property: JsonPropertyName("ItemNumber")]
    string SKU,

    [property: JsonPropertyName("ItemChannelPrices")]
    InventoryItemChannelPriceDto[] ChannelPrices,

    [property: JsonPropertyName("StockLevels")]
    InventoryItemStockLevelDto[] StockLevels
);
