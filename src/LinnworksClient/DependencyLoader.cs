using System.Net;
using LinnworksClient.Domain;
using LinnworksClient.Infrastructure;
using LinnworksClient.Infrastructure.Default;
using LinnworksClient.Infrastructure.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace LinnworksClient;

public static class DependencyLoader
{
    private static readonly IAsyncPolicy<HttpResponseMessage> DefaultRetryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(message => message.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount)));

    public static IServiceCollection RegisterLinnworksClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddMemoryCache();
        services.AddOptions<LinnworksOptions>()
            .Bind(configuration.GetSection("LinnworksConfiguration"))
            .Validate(
                x =>
                    Guid.TryParse(x.ApplicationId, out _) &&
                    Guid.TryParse(x.ApplicationSecret, out _) &&
                    Guid.TryParse(x.Token, out _),
                "Linnworks configuration is incorrect"
            );

        services.AddSingleton<ILinnworksRequestFactory, LinnworksRequestFactory>();
        services.AddSingleton<ILinnworksRateLimiterFactory, LinnworksRateLimiterFactory>();

        services.AddHttpClient<ILinnworksAuthorizationClient, LinnworksAuthorizationClient>()
            .AddPolicyHandler(DefaultRetryPolicy);

        services.AddHttpClient<ILinnworksClient, Domain.Default.LinnworksClient>()
            .AddPolicyHandler(DefaultRetryPolicy);

        return services;
    }
}