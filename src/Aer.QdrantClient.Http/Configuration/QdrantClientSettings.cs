namespace Aer.QdrantClient.Http.Configuration;

/// <summary>
/// Represents a Qdrant client configuration.
/// </summary>
public sealed class QdrantClientSettings
{
    /// <summary>
    /// The default value of http client timeout.
    /// </summary>
    public static readonly TimeSpan DefaultHttpClientTimeout = TimeSpan.FromSeconds(100);

    /// <summary>
    /// The HTTP or HTTPs host and port that Qdrant db engine listens to.
    /// </summary>
    public required string HttpAddress { set; get; }

    /// <summary>
    /// The http address as an <see cref="Uri"/> object.
    /// </summary>
    public Uri HttpAddressUri { get => field ??= new(HttpAddress); private set; }

    /// <summary>
    /// The authorization key for Qdrant db requests authorization.
    /// If not set to <c>null</c>, all requests will include a header
    /// with the api-key : <c>api-key: 'API-KEY'</c>
    /// </summary>
    public string ApiKey { set; get; }

    /// <summary>
    /// The default timeout for http client which is used to call Qdrant HTTP API.
    /// </summary>
    public TimeSpan HttpClientTimeout { set; get; } = DefaultHttpClientTimeout;

    /// <summary>
    /// If set to <c>true</c>, http client activity tracing is disabled. Default is <c>false</c>.
    /// </summary>
    public bool DisableTracing { set; get; }

    /// <summary>
    /// If set to <c>true</c> enables request \ response compression. Default is <c>false</c>.
    /// </summary>
    public bool EnableCompression { set; get; }
}
