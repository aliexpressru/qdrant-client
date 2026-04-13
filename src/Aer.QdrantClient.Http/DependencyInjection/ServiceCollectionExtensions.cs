using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.Diagnostics;
using Aer.QdrantClient.Http.Diagnostics.Listeners;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using Polly;
using Polly.CircuitBreaker;
using Polly.Telemetry;
using System.Diagnostics;

namespace Aer.QdrantClient.Http.DependencyInjection;

/// <summary>
/// The dependency injection configuration extensions class.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The default name of the Qdrant HTTP client.
    /// Used when no explicit name is provided.
    /// </summary>
    public const string DefaultQdrantHttpClientName = "DefaultQdrantHttpClient";

    /// <param name="services">The service collection to add qdrant HTTP client to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds Qdrant HTTP client to the <see cref="IServiceCollection"/> with explicitly defined settings.
        /// </summary>
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
        public IServiceCollection AddQdrantHttpClient(
            Action<QdrantClientSettings> configureQdrantClientSettings,
            CircuitBreakerStrategyOptions<HttpResponseMessage> circuitBreakerStrategyOptions = null,
            TelemetryOptions resiliencePipelineTelemetryOptions = null,
            string clientName = null,
            bool registerAsInterface = false)
        {
            var actualClientName = clientName ?? DefaultQdrantHttpClientName;

            services.Configure(actualClientName, configureQdrantClientSettings);

            AddQdrantHttpClientInternal(
                services,
                circuitBreakerStrategyOptions,
                resiliencePipelineTelemetryOptions,
                shouldSelectResiliencePipelineByAuthority: true,
                clientName: actualClientName,
                registerAsInterface);

            return services;
        }

        /// <summary>
        /// Adds Qdrant HTTP client to the <see cref="IServiceCollection"/> with settings from app configuration.
        /// </summary>
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
        public IServiceCollection AddQdrantHttpClient(
            IConfiguration configuration,
            string clientConfigurationSectionName = nameof(QdrantClientSettings),
            Action<QdrantClientSettings> configureQdrantClientSettings = null,
            CircuitBreakerStrategyOptions<HttpResponseMessage> circuitBreakerStrategyOptions = null,
            TelemetryOptions resiliencePipelineTelemetryOptions = null,
            string clientName = null,
            bool registerAsInterface = false)
        {
            var actualClientName = clientName ?? DefaultQdrantHttpClientName;

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
                clientName: actualClientName,
                registerInterface: registerAsInterface);

            return services;
        }

        /// <summary>
        /// Adds a default implementation of the <see cref="IQdrantClientFactory"/> to the service collection.
        /// </summary>
        public IServiceCollection AddQdrantClientFactory()
        {
            services.AddHttpClient();
            services.TryAddSingleton<IQdrantClientFactory, DefaultQdrantClientFactory>();

            AddTracingAndMetrics(services);

            return services;
        }
    }

    /// <param name="applicationBuilder">The application builder instance.</param>
    extension(IApplicationBuilder applicationBuilder)
    {
        /// <summary>
        /// Enables qdrant http client diagnostics listeners for metrics and logging.
        /// </summary>
        /// <param name="clientName">The name of the client diagnostic should be enabled for.</param>
        public IApplicationBuilder EnableQdrantHttpClientDiagnostics(string clientName = null)
        {
            var qdrantClientSettings =
                applicationBuilder.ApplicationServices.GetRequiredService<IOptionsSnapshot<QdrantClientSettings>>().Get(clientName ?? DefaultQdrantHttpClientName);

            if (qdrantClientSettings.DisableMetrics)
            {
                return applicationBuilder;
            }

            var diagnosticSource = applicationBuilder.ApplicationServices.GetRequiredService<QdrantHttpClientDiagnosticSource>();

            var metricsProvider = applicationBuilder.ApplicationServices.GetRequiredService<QdrantHttpClientMetricsProvider>();

            var metricsListener = new MetricsQdrantHttpClientDiagnosticListener(metricsProvider, qdrantClientSettings);

            diagnosticSource.SubscribeWithAdapter(metricsListener);

            return applicationBuilder;
        }
    }

    private static void AddQdrantHttpClientInternal(
        IServiceCollection services,
        CircuitBreakerStrategyOptions<HttpResponseMessage> circuitBreakerStrategyOptions,
        TelemetryOptions resiliencePipelineTelemetryOptions,
        bool shouldSelectResiliencePipelineByAuthority,
        string clientName,
        bool registerInterface)
    {
        void ConfigureClient(IServiceProvider serviceProvider, HttpClient client)
        {
            var scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var qdrantSettings = scopedServiceProvider
                .GetRequiredService<IOptionsSnapshot<QdrantClientSettings>>()
                .Get(clientName);

            client.BaseAddress = new Uri(qdrantSettings.HttpAddress);
            client.Timeout = qdrantSettings.HttpClientTimeout;
        }

        QdrantHttpClient ClientFactory(HttpClient client, IServiceProvider serviceProvider)
        {
            var serviceProviderScope = serviceProvider.CreateScope().ServiceProvider;

            var qdrantClientSettings = serviceProviderScope
                .GetRequiredService<IOptionsSnapshot<QdrantClientSettings>>()
                .Get(clientName);

            client.BaseAddress = new Uri(qdrantClientSettings.HttpAddress);
            client.Timeout = qdrantClientSettings.HttpClientTimeout;

            var tracer = qdrantClientSettings.DisableTracing
                ? null
                : serviceProviderScope.GetService<Tracer>();

            var loggerFactory = serviceProviderScope.GetService<ILoggerFactory>();

            return new QdrantHttpClient(
                client, qdrantClientSettings, logger: loggerFactory?.CreateLogger(nameof(QdrantHttpClient) + $"_{clientName}"), tracer: tracer);
        }

        services.AddHttpClient(clientName, ConfigureClient);

        IHttpClientBuilder httpClientBuilder = registerInterface
            ? services.AddHttpClient<IQdrantHttpClient, QdrantHttpClient>(
                    clientName,
                    ClientFactory)
            : services.AddHttpClient<QdrantHttpClient, QdrantHttpClient>(
                clientName,
                ClientFactory);

        // We use try add to avoid multiple registrations in case of registering multiple clients
        services.TryAddSingleton<IQdrantClientFactory, DefaultQdrantClientFactory>();

        httpClientBuilder.ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            var scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var qdrantSettings = scopedServiceProvider
                .GetRequiredService<IOptionsSnapshot<QdrantClientSettings>>()
                .Get(clientName);

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

        AddTracingAndMetrics(services);
    }

    private static void AddTracingAndMetrics(
        IServiceCollection services)
    {
        services.AddOpenTelemetry().WithTracing(
            builder =>
            {
                builder.AddSource(QdrantHttpClientDiagnosticConstants.TracingActivitySourceName);
            });

        // Register Tracer

        services.TryAddSingleton(TracerProvider.Default.GetTracer(QdrantHttpClientDiagnosticConstants.TracingActivityServiceName));

        // Add open telemetry metrics dependencies

        services.AddMetrics();

        services.AddOpenTelemetry().WithMetrics(
            builder =>
            {
                builder.AddMeter(QdrantHttpClientMetricsProvider.MeterName);
            });

        services.AddSingleton<QdrantHttpClientMetricsProvider>();
    }
}
