namespace Aer.QdrantClient.Http.Configuration;

/// <summary>
/// Represents a Qdrant client configuration.
/// </summary>
public class QdrantClientSettings
{
    private Uri _httpAddressUri;
    
    /// <summary>
    /// The default value of http client timeout.
    /// </summary>
    public static readonly TimeSpan DefaultHttpClientTimeout = TimeSpan.FromSeconds(100);

    /// <summary>
    /// The HTTP or HTTPs host and port that Qdrant db engine listens to.
    /// </summary>
    public required string HttpAddress { init; get; }

    /// <summary>
    /// The http address as an <see cref="Uri"/> object.
    /// </summary>
    public Uri HttpAddressUri => _httpAddressUri ??= new(HttpAddress);

    /// <summary>
    /// The authorization key for Qdrant db requests authorization.
    /// If not set to <c>null</c>, all requests will include a header
    /// with the api-key : <c>api-key: 'API-KEY'</c>
    /// </summary>
    public string ApiKey { set; get; }

    /// <summary>
    /// The default timeout for http client which ios used to call Qdrant HTTP API.
    /// </summary>
    public TimeSpan HttpClientTimeout { init; get; } = DefaultHttpClientTimeout;
}
