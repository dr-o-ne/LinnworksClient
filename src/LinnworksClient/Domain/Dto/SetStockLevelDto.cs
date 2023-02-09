using System.Text.Json.Serialization;

namespace LinnworksClient.Domain.Dto;

public sealed record SetStockLevelDto
(
    [property: JsonPropertyName("SKU")] 
    string SKU,

    [property: JsonPropertyName("LocationId")]
    string LocationId,

    [property: JsonPropertyName("Level")] 
    int Level
);
