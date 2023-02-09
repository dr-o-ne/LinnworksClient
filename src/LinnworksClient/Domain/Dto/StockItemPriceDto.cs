using System.Text.Json.Serialization;

namespace LinnworksClient.Domain.Dto;

public sealed record StockItemPriceDto
(
    [property:JsonPropertyName("pkRowId")]
    Guid ChannelId,

    [property: JsonPropertyName("Source")]
    string Source,

    [property: JsonPropertyName("SubSource")]
    string SubSource,

    [property:JsonPropertyName("StockItemId")]
    Guid StockItemId,

    [property:JsonPropertyName("Price")]
    decimal Price
);
