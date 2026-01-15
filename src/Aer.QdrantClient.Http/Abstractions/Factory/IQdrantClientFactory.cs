using Aer.QdrantClient.Http.Configuration;
using Aer.QdrantClient.Http.Exceptions;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Http.Abstractions;

/// <summary>
/// Represents a factory for creating <see cref="IQdrantHttpClient"/> instances.
/// </summary>
public interface IQdrantClientFactory
{
    /// <summary>
    /// Adds a new client configuration with the specified name and settings.
    /// </summary>
    /// <param name="clientName">The client name.</param>
    /// <param name="settings">The client settings.</param>
    /// <param name="logger">
    /// An optional logger instance for capturing diagnostic and operational logs. If null, logging is disabled for this
    /// client configuration.
    /// </param>
    void AddClientConfiguration(
        string clientName,
        QdrantClientSettings settings,
        ILogger logger = null);

    /// <summary>
    /// Adds a client configuration for connecting to a remote service using the specified settings.
    /// </summary>
    /// <remarks>
    /// If a configuration with the same client name already exists, this method may overwrite or
    /// update the existing configuration depending on implementation. Tracing and compression options affect network
    /// diagnostics and performance.
    /// </remarks>
    /// <param name="clientName">The unique name that identifies the client configuration. Cannot be null or empty.</param>
    /// <param name="httpAddress">The HTTP address of the remote service endpoint. Must be a valid absolute URI.</param>
    /// <param name="apiKey">
    /// An optional API key used for authenticating requests to the remote service. If null, authentication may be
    /// disabled or handled differently depending on the service.
    /// </param>
    /// <param name="httpClientTimeout">An optional timeout value for HTTP requests made by the client. If <c>null</c>, the default timeout is used.</param>
    /// <param name="logger">
    /// An optional logger instance for capturing diagnostic and operational logs. If null, logging is disabled for this
    /// client configuration.
    /// </param>
    /// <param name="disableTracing">
    /// Specifies whether distributed tracing is disabled for this client. Set to <c>true</c> to disable
    /// tracing; otherwise, tracing is enabled.
    /// </param>
    /// <param name="enableCompression">
    /// Specifies whether HTTP request and response compression is enabled. Set to <c>true</c> to enable
    /// compression; otherwise, compression is disabled.
    /// </param>
    void AddClientConfiguration(
        string clientName,
        string httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false);

    /// <summary>
    /// Adds a client configuration for connecting to a remote service using the specified settings.
    /// </summary>
    /// <remarks>
    /// If a configuration with the same client name already exists, this method may overwrite or
    /// update the existing configuration depending on implementation. Tracing and compression options affect network
    /// diagnostics and performance.
    /// </remarks>
    /// <param name="clientName">The unique name that identifies the client configuration. Cannot be null or empty.</param>
    /// <param name="httpAddress">The HTTP address of the remote service endpoint. Must be a valid absolute URI.</param>
    /// <param name="apiKey">
    /// An optional API key used for authenticating requests to the remote service. If null, authentication may be
    /// disabled or handled differently depending on the service.
    /// </param>
    /// <param name="httpClientTimeout">An optional timeout value for HTTP requests made by the client. If <c>null</c>, the default timeout is used.</param>
    /// <param name="logger">
    /// An optional logger instance for capturing diagnostic and operational logs. If null, logging is disabled for this
    /// client configuration.
    /// </param>
    /// <param name="disableTracing">
    /// Specifies whether distributed tracing is disabled for this client. Set to <c>true</c> to disable
    /// tracing; otherwise, tracing is enabled.
    /// </param>
    /// <param name="enableCompression">
    /// Specifies whether HTTP request and response compression is enabled. Set to <c>true</c> to enable
    /// compression; otherwise, compression is disabled.
    /// </param>
    public void AddClientConfiguration(
        string clientName,
        Uri httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false);

    /// <summary>
    /// Adds a client configuration for connecting to a remote service endpoint with the specified settings.
    /// </summary>
    /// <remarks>
    /// If a client configuration with the same name already exists, this method may overwrite the
    /// existing configuration. All parameters should be set according to the requirements of the target service
    /// endpoint.
    /// </remarks>
    /// <param name="clientName">The unique name used to identify the client configuration. Cannot be <c>null</c> or empty.</param>
    /// <param name="host">The host name or IP address of the remote service endpoint. Cannot be <c>null</c> or empty.</param>
    /// <param name="port">The port number to use when connecting to the remote service. Must be a valid TCP port number. The default is <c>6334</c>.</param>
    /// <param name="useHttps">Specifies whether to use HTTPS for the connection. Set to <c>true</c> to enable HTTPS; otherwise, <c>false</c>.</param>
    /// <param name="apiKey">
    /// The API key used for authenticating requests to the remote service.
    /// Can be <c>null</c> if authentication is not
    /// required.</param>
    /// <param name="httpClientTimeout">The maximum duration to wait for HTTP requests to complete. If <c>null</c>, the default timeout is used.</param>
    /// <param name="logger">An optional logger instance for capturing diagnostic information. If <c>null</c>, logging is disabled for this client.</param>
    /// <param name="disableTracing">
    /// Specifies whether distributed tracing is disabled for this client. Set to <c>true</c> to disable
    /// tracing; otherwise, <c>false</c>.
    /// </param>
    /// <param name="enableCompression">
    /// Specifies whether HTTP request and response compression is enabled. Set to <c>true</c> to enable
    /// compression; otherwise, <c>false</c>.
    /// </param>
    void AddClientConfiguration(
        string clientName,
        string host,
        int port = 6334,
        bool useHttps = false,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false);

    /// <summary>
    /// Creates a new instance of <see cref="IQdrantHttpClient"/> with the specified client name.
    /// For client to be created, it must either be previously registered in the dependency
    /// injection container with the same name it is going to be requested by using one of the
    /// <c>ServiceCollectionExtensions.AddQdrantHttpClient</c> overloads.
    /// 
    /// Or it must be registered using one of the <c>IQdrantClientFactory.AddClientConfiguration</c> overloads.
    /// If no named configuration is found, an exception of type <see cref="QdrantNamedQdrantClientNotFound"/> is thrown.
    /// </summary>
    /// <param name="clientName">The name of the client to get.</param>
    /// <exception cref="QdrantNamedQdrantClientNotFound">
    /// Thrown when the client configuration was not registered either by directly calling to
    /// factory or via <c>ServiceCollectionExtensions.AddQdrantHttpClient</c> overloads.
    /// </exception>
    IQdrantHttpClient CreateClient(string clientName);

    /// <summary>
    /// Creates a new instance of <see cref="IQdrantHttpClient"/> with the specified client settings.
    /// </summary>
    /// <param name="settings">The client settings to use for the new client instance.</param>
    /// <param name="logger">An optional logger instance for capturing diagnostic information.</param>
    IQdrantHttpClient CreateClient(QdrantClientSettings settings, ILogger logger = null);

    /// <summary>
    /// Creates a new instance of <see cref="IQdrantHttpClient"/> with the specified client settings.
    /// </summary>
    /// <param name="httpAddress">The HTTP address of the remote service endpoint. Must be a valid absolute URI.</param>
    /// <param name="apiKey">
    /// An optional API key used for authenticating requests to the remote service. If null, authentication may be
    /// disabled or handled differently depending on the service.
    /// </param>
    /// <param name="httpClientTimeout">An optional timeout value for HTTP requests made by the client. If <c>null</c>, the default timeout is used.</param>
    /// <param name="logger">
    /// An optional logger instance for capturing diagnostic and operational logs. If null, logging is disabled for this
    /// client configuration.
    /// </param>
    /// <param name="disableTracing">
    /// Specifies whether distributed tracing is disabled for this client. Set to <c>true</c> to disable
    /// tracing; otherwise, tracing is enabled.
    /// </param>
    /// <param name="enableCompression">
    /// Specifies whether HTTP request and response compression is enabled. Set to <c>true</c> to enable
    /// compression; otherwise, compression is disabled.
    /// </param>
    IQdrantHttpClient CreateClient(
        Uri httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false);

    /// <summary>
    /// Creates a new instance of <see cref="IQdrantHttpClient"/> with the specified client settings.
    /// </summary>
    /// <param name="host">The host name or IP address of the remote service endpoint. Cannot be <c>null</c> or empty.</param>
    /// <param name="port">The port number to use when connecting to the remote service. Must be a valid TCP port number. The default is <c>6334</c>.</param>
    /// <param name="useHttps">Specifies whether to use HTTPS for the connection. Set to <c>true</c> to enable HTTPS; otherwise, <c>false</c>.</param>
    /// <param name="apiKey">
    /// The API key used for authenticating requests to the remote service.
    /// Can be <c>null</c> if authentication is not
    /// required.</param>
    /// <param name="httpClientTimeout">The maximum duration to wait for HTTP requests to complete. If <c>null</c>, the default timeout is used.</param>
    /// <param name="logger">An optional logger instance for capturing diagnostic information. If <c>null</c>, logging is disabled for this client.</param>
    /// <param name="disableTracing">
    /// Specifies whether distributed tracing is disabled for this client. Set to <c>true</c> to disable
    /// tracing; otherwise, <c>false</c>.
    /// </param>
    /// <param name="enableCompression">
    /// Specifies whether HTTP request and response compression is enabled. Set to <c>true</c> to enable
    /// compression; otherwise, <c>false</c>.
    /// </param>
    IQdrantHttpClient CreateClient(
        string host,
        int port = 6334,
        bool useHttps = false,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false);

    /// <summary>
    /// Creates a new instance of <see cref="IQdrantHttpClient"/> with the specified client settings.
    /// </summary>
    /// <param name="httpAddress">The HTTP address of the remote service endpoint. Must be a valid absolute URI.</param>
    /// <param name="apiKey">
    /// An optional API key used for authenticating requests to the remote service. If null, authentication may be
    /// disabled or handled differently depending on the service.
    /// </param>
    /// <param name="httpClientTimeout">An optional timeout value for HTTP requests made by the client. If <c>null</c>, the default timeout is used.</param>
    /// <param name="logger">
    /// An optional logger instance for capturing diagnostic and operational logs. If null, logging is disabled for this
    /// client configuration.
    /// </param>
    /// <param name="disableTracing">
    /// Specifies whether distributed tracing is disabled for this client. Set to <c>true</c> to disable
    /// tracing; otherwise, tracing is enabled.
    /// </param>
    /// <param name="enableCompression">
    /// Specifies whether HTTP request and response compression is enabled. Set to <c>true</c> to enable
    /// compression; otherwise, compression is disabled.
    /// </param>
    IQdrantHttpClient CreateClient(
        string httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        ILogger logger = null,
        bool disableTracing = false,
        bool enableCompression = false);

    /// <summary>
    /// Creates a plain <see cref="HttpClient"/> instance configured for making calls to Qdrant HTTP api.
    /// This method can be used to create a raw HTTP client for making custom requests to the Qdrant API or to override default client,
    /// configured for <see cref="IQdrantHttpClient"/> upon creation.
    /// </summary>
    /// <param name="httpAddress">The HTTP address of the remote service endpoint. Must be a valid absolute URI.</param>
    /// <param name="apiKey">
    /// An optional API key used for authenticating requests to the remote service. If null, authentication may be
    /// disabled or handled differently depending on the service.
    /// </param>
    /// <param name="httpClientTimeout">An optional timeout value for HTTP requests made by the client. If <c>null</c>, the default timeout is used.</param>
    /// <param name="disableTracing">
    /// Specifies whether distributed tracing is disabled for this client. Set to <c>true</c> to disable
    /// tracing; otherwise, tracing is enabled.
    /// </param>
    /// <param name="enableCompression">
    /// Specifies whether HTTP request and response compression is enabled. Set to <c>true</c> to enable
    /// compression; otherwise, compression is disabled.
    /// </param>
    HttpClient CreateApiClient(Uri httpAddress,
        string apiKey = null,
        TimeSpan? httpClientTimeout = null,
        bool disableTracing = false,
        bool enableCompression = false);

    /// <summary>
    /// Gets a plain <see cref="HttpClient"/> instance configured for making calls to Qdrant HTTP api.
    ///
    /// This method can be used to obtain a raw HTTP client for making custom requests to the Qdrant API or to override default client,
    /// configured for <see cref="IQdrantHttpClient"/> upon creation.
    /// 
    /// For client to be obtained, it must either be previously registered in the dependency
    /// injection container with the same name it is going to be requested by using one of the
    /// <c>ServiceCollectionExtensions.AddQdrantHttpClient</c> overloads.
    /// 
    /// Or it must be registered using one of the <c>IQdrantClientFactory.AddClientConfiguration</c> overloads.
    /// If no named configuration is found, an exception of type <see cref="QdrantNamedQdrantClientNotFound"/> is thrown.
    /// </summary>
    /// <param name="clientName">The name of the client to get.</param>
    /// <exception cref="QdrantNamedQdrantClientNotFound">
    /// Thrown when the client configuration was not registered either by directly calling to
    /// factory or via <c>ServiceCollectionExtensions.AddQdrantHttpClient</c> overloads.
    /// </exception>
    HttpClient GetApiClient(string clientName);
}
