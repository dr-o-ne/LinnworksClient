using System.Text.Json.Serialization;

namespace LinnworksClient.Domain.Dto;

public sealed record SetOrderShippingInfoDto
(
    [property: JsonPropertyName("PostalServiceId")]
    Guid PostalServiceId,

    [property: JsonPropertyName("TrackingNumber")]
    string TrackingNumber,

    [property: JsonPropertyName("ManualAdjust")]
    bool ManualAdjust = true
);