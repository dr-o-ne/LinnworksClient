namespace LinnworksClient.Infrastructure.Dto;

internal sealed class LinnworksOptions
{
    public Uri Url { get; set; } = null!;
    public string ApplicationId { get; set; } = null!;
    public string ApplicationSecret { get; set; } = null!;
    public string Token { get; set; } = null!;
}