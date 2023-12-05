using Aer.QdrantClient.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable MemberCanBeInternal

namespace Aer.QdrantClient.Http.DependencyInjection;

/// <summary>
/// The dependency injection configuration extensions class.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Qdrant HTTP client to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add qdrant HTTP client to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="clientConfigurationSectionName">The name of the appsettings.json file section where <see cref="QdrantClientSettings"/> is configured.</param>
    /// <param name="configureOptions">The configure the <see cref="QdrantClientSettings"/> parameters action.</param>
    public static IServiceCollection AddQdrantHttpClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string clientConfigurationSectionName = nameof(QdrantClientSettings),
        Action<QdrantClientSettings> configureOptions = null)
    {
        services.Configure<QdrantClientSettings>(configuration.GetSection(clientConfigurationSectionName));

        if (configureOptions is not null)
        {
            services.PostConfigure(configureOptions);
        }

        services.AddHttpClient<QdrantHttpClient, QdrantHttpClient>(
            static (serviceProvider, client) =>
            {
                var qdrantSettings =
                    serviceProvider.GetRequiredService<IOptions<QdrantClientSettings>>().Value;

                client.BaseAddress = new Uri(qdrantSettings.HttpAddress);
                client.Timeout = qdrantSettings.HttpClientTimeout;

                if (qdrantSettings.ApiKey is {Length: > 0})
                {
                    client.DefaultRequestHeaders.Add(
                        "api-key",
                        qdrantSettings.ApiKey
                    );
                }
            }
        );

        return services;
    }
}
