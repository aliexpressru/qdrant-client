using System.Linq.Expressions;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;

// ReSharper disable MemberCanBeInternal

namespace Aer.QdrantClient.Http.DependencyInjection;

/// <summary>
/// The dependency injection configuration extensions class.
/// </summary>
public static class ServiceCollectionExtensions
{
    internal static bool IsResiliencePipelineRetryStrategyConfigured { get; set; }

    /// <summary>
    /// Adds Qdrant HTTP client to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add qdrant HTTP client to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="clientConfigurationSectionName">The name of the appsettings.json file section where <see cref="QdrantClientSettings"/> is configured.</param>
    /// <param name="configureOptions">The configure the <see cref="QdrantClientSettings"/> parameters action.</param>
    /// <param name="resiliencePipelineName">The optional resilience pipeline name. Default value is <c>QdrantHttpClientResiliencePipeline</c>.</param>
    /// <param name="configureResiliencePipeline">
    /// The optional action to configure resilience options such as circuit breaker or custom retries.
    /// If you configure retry strategy this way, then the retry-related parameters on the API calls will be ignored. 
    /// </param>
    public static IServiceCollection AddQdrantHttpClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string clientConfigurationSectionName = nameof(QdrantClientSettings),
        Action<QdrantClientSettings> configureOptions = null,
        string resiliencePipelineName = "QdrantHttpClientResiliencePipeline",
        Expression<Action<ResiliencePipelineBuilder<HttpResponseMessage>>> configureResiliencePipeline = null)
    {
        services.Configure<QdrantClientSettings>(configuration.GetSection(clientConfigurationSectionName));

        if (configureOptions is not null)
        {
            services.PostConfigure(configureOptions);
        }

        if (configureResiliencePipeline is not null)
        {
            var configureResiliencePipelineAction = configureResiliencePipeline.Compile();
            
            services.AddHttpClient<QdrantHttpClient, QdrantHttpClient>(static (serviceProvider, client) =>
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
            ).AddResilienceHandler(resiliencePipelineName, configureResiliencePipelineAction);

            IsResiliencePipelineRetryStrategyConfigured =
                ReflectionHelper.CheckRetryStrategyConfigured(configureResiliencePipeline);
        }
        else
        {
            services.AddHttpClient<QdrantHttpClient, QdrantHttpClient>(static (serviceProvider, client) =>
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
        }

        return services;
    }
}
