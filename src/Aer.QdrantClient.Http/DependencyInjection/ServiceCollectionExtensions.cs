using Aer.QdrantClient.Http.Abstractions;
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
    internal const string DefaultHttpClientName = "DefaultQdrantHttpClient";

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
    /// <param name="clientName">The name of the client to be registered.</param>
    /// <param name="registerAsInterface">
    /// If set to <c>true</c> registers <see cref="IQdrantHttpClient"/> interface instead of concrete <see cref="QdrantHttpClient"/>.
    /// This setting exists for backwards compatibility, since concrete registration was default behaviour in versions before 1.15.13.
    /// </param>
    public static IServiceCollection AddQdrantHttpClient(
        this IServiceCollection services,
        Action<QdrantClientSettings> configureQdrantClientSettings,
        CircuitBreakerStrategyOptions<HttpResponseMessage> circuitBreakerStrategyOptions = null,
        TelemetryOptions resiliencePipelineTelemetryOptions = null,
        string clientName = null,
        bool registerAsInterface = false)
    {
        var actualClientName = clientName ?? DefaultHttpClientName;
        
        services.Configure(actualClientName, configureQdrantClientSettings);
        
        AddQdrantHttpClientInternal(
            services,
            circuitBreakerStrategyOptions,
            resiliencePipelineTelemetryOptions,
            shouldSelectResiliencePipelineByAuthority: true,
            clientName ?? DefaultHttpClientName,
            registerAsInterface);

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
    /// <param name="clientName">The name of the client to be registered.</param>
    /// <param name="registerAsInterface">
    /// If set to <c>true</c> registers <see cref="IQdrantHttpClient"/> interface instead of concrete <see cref="QdrantHttpClient"/>.
    /// This setting exists for backwards compatibility, since concrete registration was default behaviour in versions before 1.15.13.
    /// </param>
    public static IServiceCollection AddQdrantHttpClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string clientConfigurationSectionName = nameof(QdrantClientSettings),
        Action<QdrantClientSettings> configureQdrantClientSettings = null,
        CircuitBreakerStrategyOptions<HttpResponseMessage> circuitBreakerStrategyOptions = null,
        TelemetryOptions resiliencePipelineTelemetryOptions = null,
        string clientName = null,
        bool registerAsInterface = false)
    {
        var actualClientName = clientName ?? DefaultHttpClientName;
        
        services.Configure<QdrantClientSettings>(actualClientName, configuration.GetSection(clientConfigurationSectionName));

        if (configureQdrantClientSettings is not null)
        {
            services.PostConfigure(actualClientName, configureQdrantClientSettings);
        }

        AddQdrantHttpClientInternal(
            services,
            circuitBreakerStrategyOptions,
            resiliencePipelineTelemetryOptions,
            shouldSelectResiliencePipelineByAuthority: true,
            actualClientName,
            registerInterface: registerAsInterface);

        return services;
    }

    private static void AddQdrantHttpClientInternal(
        IServiceCollection services,
        CircuitBreakerStrategyOptions<HttpResponseMessage> circuitBreakerStrategyOptions,
        TelemetryOptions resiliencePipelineTelemetryOptions,
        bool shouldSelectResiliencePipelineByAuthority,
        string clientName,
        bool registerInterface)
    {
        Action<IServiceProvider, HttpClient> configureClient = (serviceProvider, client) =>
        {
            var qdrantSettings =
                serviceProvider.GetRequiredService<IOptionsSnapshot<QdrantClientSettings>>().Get(clientName);

            client.BaseAddress = new Uri(qdrantSettings.HttpAddress);
            client.Timeout = qdrantSettings.HttpClientTimeout;
        };

        IHttpClientBuilder httpClientBuilder = registerInterface
            ? services.AddHttpClient<IQdrantHttpClient, QdrantHttpClient>(
                    clientName,
                    configureClient)
            : services.AddHttpClient<QdrantHttpClient, QdrantHttpClient>(
                clientName,
                configureClient);

        httpClientBuilder.ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            var qdrantSettings =
                serviceProvider.GetRequiredService<IOptionsSnapshot<QdrantClientSettings>>().Get(clientName);
            
            var handler = QdrantHttpClient.CreateHttpClientHandler(
                isCompressionEnabled: qdrantSettings.EnableCompression,
                isDisableTracing: qdrantSettings.DisableTracing,
                apiKey: qdrantSettings.ApiKey);
            
            return handler;
        });

        if (circuitBreakerStrategyOptions is null)
        {
            return;
        }

        IHttpResiliencePipelineBuilder resiliencePipelineBuilder = httpClientBuilder
            .AddResilienceHandler(
                $"QdrantHttpClientResiliencePipeline_{clientName}",
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
