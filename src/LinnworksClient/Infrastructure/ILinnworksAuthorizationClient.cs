using LinnworksClient.Infrastructure.Dto;

namespace LinnworksClient.Infrastructure;

internal interface ILinnworksAuthorizationClient
{
    Task<LinnworksSession?> Authorize(CancellationToken cancellationToken);
}