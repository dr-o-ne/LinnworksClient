using System.Text.Json.Serialization;

namespace LinnworksClient.Domain.Dto;

public sealed record InventoryItemStockLevelDto
(
    [property: JsonPropertyName("StockLevel")]
    int StockLevel
);