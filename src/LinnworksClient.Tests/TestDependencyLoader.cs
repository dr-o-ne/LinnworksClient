using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinnworksClient.Tests;

internal static class TestDependencyLoader
{
    public static IServiceCollection RegisterTestLinnworksClient(this IServiceCollection services, string serverUrl)
    {
        var configValues = new Dictionary<string, string>
        {
            {"LinnworksConfiguration:Url", serverUrl},
            {"LinnworksConfiguration:ApplicationId", Guid.Empty.ToString()},
            {"LinnworksConfiguration:ApplicationSecret", Guid.Empty.ToString()},
            {"LinnworksConfiguration:Token", Guid.Empty.ToString()}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        services.RegisterLinnworksClient(configuration);

        return services;
    }
}