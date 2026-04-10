using Aer.QdrantClient.Http.Abstractions;
using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;

namespace Aer.QdrantClient.Http.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="IQdrantClientFactory"/>.
/// </summary>
internal class DefaultQdrantClientFactory(IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
    : IQdrantClientFactory
{
    private class StoredQdrantClientSettings()
    {
        public required Uri QdrantAddress { init; get; }

        public string ApiKey { init; get; }

        public TimeSpan HttpClientTimeout { init; get; } = QdrantClientSettings.DefaultHttpClientTimeout;

        public bool DisableTracing { init; get; }

        public bool EnableCompression { init; get; }

        public ILogger Logger { init; get; }

        public Tracer Tracer { init; get; }
    }

    readonly HashSet<string> _unregisteredClientNames = [];

    readonly Dictionary<string, StoredQdrantClientSettings> _clientSettings = [];

    /// <inheritdoc/>
    public void AddClientConfiguration(
        string clientName,
        QdrantClientSettings settings,
        ILogger logger = null,
        Tracer tracer = null
    )
    {
        _clientSettings[clientName] = new()
        {
            QdrantAddress = new Uri(settings.HttpAddress),
            ApiKey = settings.ApiKey,
            HttpClientTimeout = settings.HttpClientTimeout,
            DisableTracing = settings.DisableTracing,
            EnableCompression = settings.EnableCompression,
            Logger = logger,
            Tracer = tracer,
        };
    }

    /// <inheritdoc/>
    public void AddClientConfiguration(
        string clientName,
        Uri httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false,
        Tracer tracer = null
    )
    {
        var settings = new StoredQdrantClientSettings()
        {
            QdrantAddress = httpAddress,
            ApiKey = apiKey,
            HttpClientTimeout = httpClientTimeout ?? QdrantClientSettings.DefaultHttpClientTimeout,
            DisableTracing = disableTracing,
            EnableCompression = enableCompression,

            Logger = logger,
            Tracer = tracer,
        };

        _clientSettings[clientName] = settings;
    }

    /// <inheritdoc/>
    public void AddClientConfiguration(
        string clientName,
        string httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false,
        Tracer tracer = null
    ) =>
        AddClientConfiguration(
            clientName,
            new Uri(httpAddress),
            apiKey,
            httpClientTimeout,
            logger,
            disableTracing: disableTracing,
            enableCompression: enableCompression,
            tracer: tracer
        );

    /// <inheritdoc/>
    public void AddClientConfiguration(
        string clientName,
        string host,
        int port = 6334,
        bool useHttps = false,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false,
        Tracer tracer = null
    ) =>
        AddClientConfiguration(
            clientName,
            new UriBuilder(useHttps ? "https" : "http", host, port).Uri,
            apiKey,
            httpClientTimeout,
            logger,
            disableTracing: disableTracing,
            enableCompression: enableCompression,
            tracer: tracer
        );

    /// <inheritdoc/>
    public IQdrantHttpClient CreateClient(string clientName)
    {
        // If we have already determined that this client name is not registered, throw immediately
        if (_unregisteredClientNames.Contains(clientName))
        {
            throw new QdrantNamedQdrantClientNotFound(clientName);
        }

        // Check if we have stored settings for this client name
        if (_clientSettings.TryGetValue(clientName, out StoredQdrantClientSettings settings))
        {
            return new QdrantHttpClient(
                settings.QdrantAddress,
                settings.ApiKey,
                settings.HttpClientTimeout,
                settings.Logger,
                disableTracing: settings.DisableTracing,
                enableCompression: settings.EnableCompression,
                tracer: settings.Tracer
            );
        }
        else
        {
            // Means we are trying to created a client that has been registered in DI
            var httpClient = httpClientFactory.CreateClient(clientName);

            if (httpClient.BaseAddress == null)
            {
                // Means that no HttpClient was registered with such name
                _ = _unregisteredClientNames.Add(clientName);
                throw new QdrantNamedQdrantClientNotFound(clientName);
            }

            var scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;

            var clientSettings = scopedServiceProvider
                .GetRequiredService<IOptionsSnapshot<QdrantClientSettings>>()
                .Get(clientName);

            Tracer tracer = null;

            if (!clientSettings.DisableTracing)
            {
                tracer = scopedServiceProvider.GetService<Tracer>();
            }

            var loggerFactory = scopedServiceProvider.GetService<ILoggerFactory>();

            return new QdrantHttpClient(
                httpClient,
                clientSettings,
                logger: loggerFactory?.CreateLogger(nameof(QdrantHttpClient) + $"_{clientName}"),
                tracer: tracer
            );
        }
    }

    /// <inheritdoc/>
    public IQdrantHttpClient CreateClient(QdrantClientSettings settings, ILogger logger = null, Tracer tracer = null) =>
        new QdrantHttpClient(
            new Uri(settings.HttpAddress),
            settings.ApiKey,
            settings.HttpClientTimeout,
            logger,
            disableTracing: settings.DisableTracing,
            enableCompression: settings.EnableCompression,
            tracer: tracer
        );

    /// <inheritdoc/>
    public IQdrantHttpClient CreateClient(
        Uri httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false,
        Tracer tracer = null
    ) =>
        new QdrantHttpClient(
            httpAddress,
            apiKey,
            httpClientTimeout ?? QdrantClientSettings.DefaultHttpClientTimeout,
            logger,
            disableTracing: disableTracing,
            enableCompression: enableCompression,
            tracer: tracer
        );

    /// <inheritdoc/>
    public IQdrantHttpClient CreateClient(
        string host,
        int port = 6334,
        bool useHttps = false,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false,
        Tracer tracer = null
    ) =>
        CreateClient(
            new UriBuilder(useHttps ? "https" : "http", host, port).Uri,
            apiKey,
            httpClientTimeout,
            logger,
            disableTracing: disableTracing,
            enableCompression: enableCompression,
            tracer: tracer
        );

    /// <inheritdoc/>
    public IQdrantHttpClient CreateClient(
        string httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false,
        Tracer tracer = null
    ) =>
        CreateClient(
            new Uri(httpAddress),
            apiKey,
            httpClientTimeout,
            logger,
            disableTracing: disableTracing,
            enableCompression: enableCompression,
            tracer: tracer
        );

    /// <inheritdoc/>
    public HttpClient CreateApiClient(
        Uri httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        bool disableTracing = false,
        bool enableCompression = false
    )
    {
        var httpApiClient = QdrantHttpClient.CreateApiClient(
            httpAddress,
            apiKey,
            httpClientTimeout,
            disableTracing: disableTracing,
            enableCompression: enableCompression
        );

        return httpApiClient;
    }

    /// <inheritdoc/>
    public HttpClient GetApiClient(string clientName)
    {
        // If we have already determined that this client name is not registered, throw immediately
        if (_unregisteredClientNames.Contains(clientName))
        {
            throw new QdrantNamedQdrantClientNotFound(clientName);
        }

        // Check if we have stored settings for this client name
        if (_clientSettings.TryGetValue(clientName, out StoredQdrantClientSettings settings))
        {
            var httpApiClient = QdrantHttpClient.CreateApiClient(
                settings.QdrantAddress,
                settings.ApiKey,
                settings.HttpClientTimeout,
                disableTracing: settings.DisableTracing,
                enableCompression: settings.EnableCompression
            );

            return httpApiClient;
        }
        else
        {
            // Means we are trying to created a client that has been registered in DI
            var httpApiClient = httpClientFactory.CreateClient(clientName);

            if (httpApiClient.BaseAddress == null)
            {
                // Means that no HttpClient was registered with such name
                _ = _unregisteredClientNames.Add(clientName);
                throw new QdrantNamedQdrantClientNotFound(clientName);
            }

            return httpApiClient;
        }
    }
}
