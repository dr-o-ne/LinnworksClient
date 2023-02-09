namespace LinnworksClient.Infrastructure.Dto;

public sealed class LinnworksRateLimitException : Exception
{
    public LinnworksRateLimitException(string message, Exception ex) : base(message, ex)
    {
    }
}
