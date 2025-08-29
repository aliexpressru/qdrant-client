using Aer.QdrantClient.Http.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Telemetry;

namespace Aer.QdrantClient.Http.DependencyInjection;

/// <summary>
/// The dependency injection configuration extensions class.
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string DEFAULT_HTTP_CLIENT_NAME = "DefaultQdrantHttpClient";

    /// <summary>
    /// Adds Qdrant HTTP client to the <see cref="IServiceCollection"/> with explicitly defined settings.
    /// </summary>
    /// <param name="services">The service collection to add qdrant HTTP client to.</param>
    /// <param name="configureQdrantClientSettings">
    /// The action to configure the <see cref="QdrantClientSettings"/> parameters.
    /// </param>
    /// <param name="circuitBreakerStrategyOptions">
    /// If set, configures the circuit breaker strategy for all qdrant backend calls with specified options.
    /// The circuit breaker strategy is cached by authority (scheme + host + port).
    /// </param>
    /// <param name="resiliencePipelineTelemetryOptions">
    /// The resilience pipeline telemetry configuration.
    /// Configures telemetry for circuit breaker.
    /// </param>
    public static IServiceCollection AddQdrantHttpClient(
        this IServiceCollection services,
        Action<QdrantClientSettings> configureQdrantClientSettings,
        CircuitBreakerStrategyOptions<HttpResponseMessage> circuitBreakerStrategyOptions = null,
        TelemetryOptions resiliencePipelineTelemetryOptions = null)
    {
        services.Configure(configureQdrantClientSettings);
        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<QdrantClientSettings>>().Value);

        AddQdrantHttpClientInternal(
            services,
            circuitBreakerStrategyOptions,
            resiliencePipelineTelemetryOptions,
            shouldSelectResiliencePipelineByAuthority: true);

        return services;
    }

    /// <summary>
    /// Adds Qdrant HTTP client to the <see cref="IServiceCollection"/> with settings from app configuration.
    /// </summary>
    /// <param name="services">The service collection to add qdrant HTTP client to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="clientConfigurationSectionName">The name of the appsettings.json file section where <see cref="QdrantClientSettings"/> is configured.</param>
    /// <param name="configureQdrantClientSettings">
    /// The action to modify the <see cref="QdrantClientSettings"/> parameters after they are obtained from appsettings.json.
    /// </param>
    /// <param name="circuitBreakerStrategyOptions">
    /// If set, configures the circuit breaker strategy for all qdrant backend calls with specified options.
    /// The circuit breaker strategy is cached by authority (scheme + host + port).
    /// </param>
    /// <param name="resiliencePipelineTelemetryOptions">
    /// The resilience pipeline telemetry configuration.
    /// Configures telemetry for circuit breaker.
    /// </param>
    public static IServiceCollection AddQdrantHttpClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string clientConfigurationSectionName = nameof(QdrantClientSettings),
        Action<QdrantClientSettings> configureQdrantClientSettings = null,
        CircuitBreakerStrategyOptions<HttpResponseMessage> circuitBreakerStrategyOptions = null,
        TelemetryOptions resiliencePipelineTelemetryOptions = null)
    {
        services.Configure<QdrantClientSettings>(configuration.GetSection(clientConfigurationSectionName));
        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<QdrantClientSettings>>().Value);

        if (configureQdrantClientSettings is not null)
        {
            services.PostConfigure(configureQdrantClientSettings);
        }

        AddQdrantHttpClientInternal(
            services,
            circuitBreakerStrategyOptions,
            resiliencePipelineTelemetryOptions,
            shouldSelectResiliencePipelineByAuthority: true);

        return services;
    }

    private static void AddQdrantHttpClientInternal(
        IServiceCollection services,
        CircuitBreakerStrategyOptions<HttpResponseMessage> circuitBreakerStrategyOptions,
        TelemetryOptions resiliencePipelineTelemetryOptions,
        bool shouldSelectResiliencePipelineByAuthority)
    {
        IHttpClientBuilder httpClientBuilder = services
            .AddHttpClient<QdrantHttpClient, QdrantHttpClient>(
                DEFAULT_HTTP_CLIENT_NAME,
                static (serviceProvider, client) =>
                {
                    var qdrantSettings =
                        serviceProvider.GetRequiredService<QdrantClientSettings>();

                    client.BaseAddress = new Uri(qdrantSettings.HttpAddress);
                    client.Timeout = qdrantSettings.HttpClientTimeout;

                    if (qdrantSettings.ApiKey is {Length: > 0})
                    {
                        client.DefaultRequestHeaders.Add(
                            QdrantHttpClient.ApiKeyHeaderName,
                            qdrantSettings.ApiKey
                        );
                    }
                }
            );

        if (circuitBreakerStrategyOptions is null)
        {
            return;
        }

        IHttpResiliencePipelineBuilder resiliencePipelineBuilder = httpClientBuilder
            .AddResilienceHandler(
                "QdrantHttpClientResiliencePipeline",
                builder =>
                {
                    builder.AddCircuitBreaker(circuitBreakerStrategyOptions);
                    if (resiliencePipelineTelemetryOptions is not null)
                    {
                        builder.ConfigureTelemetry(resiliencePipelineTelemetryOptions);
                    }
                });

        if (shouldSelectResiliencePipelineByAuthority)
        {
            resiliencePipelineBuilder.SelectPipelineByAuthority();
        }
    }
}
